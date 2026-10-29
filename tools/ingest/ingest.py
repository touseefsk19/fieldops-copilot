"""FieldOps manual ingestion: check new manuals, publish them, re-index, and gate on the retrieval eval.

Only the Python standard library is used, so there is nothing to pip install.

Usage (from the repo root, API running):
    python3 tools/ingest/ingest.py tools/ingest/samples --check-only
    export FIELDOPS_TOKEN=$(dotnet user-jwts create --project src/FieldOps.Api --name boss --role supervisor --output token)
    python3 tools/ingest/ingest.py tools/ingest/samples
    python3 tools/ingest/ingest.py some/folder --audience supervisor
"""
from __future__ import annotations

import argparse
import json
import os
import re
import shutil
import sys
import urllib.error
import urllib.request
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
DEFAULT_MANUALS = REPO_ROOT / "src" / "FieldOps.Api" / "Manuals"
MAX_SECTION_CHARS = 1500   # one "## " section = one chunk; huge chunks make retrieval worse

# Same rules as the API's PiiRedactor: emails, and numbers with 10+ digits (phones, accounts)
EMAIL = re.compile(r"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}")
LONG_NUMBER = re.compile(r"\+?\d[\d\s-]{8,}\d")
# "## 4.2 Coupling bolts" or "## 1. Spend approval limits"
SECTION = re.compile(r"^## \d+(\.\d+)*\.?\s+\S")


def check_manual(name: str, text: str) -> list[str]:
    """Returns a list of problems. An empty list means the manual is OK to publish."""
    problems: list[str] = []
    lines = text.splitlines()

    first = next((line for line in lines if line.strip()), "")
    if not first.startswith("# "):
        problems.append(f"{name}: the first line must be a title starting with '# '")

    sections: dict[str, list[str]] = {}   # heading -> its lines
    current = None
    for number, line in enumerate(lines, start=1):
        if line.startswith("## "):
            current = line[3:].strip()
            if not SECTION.match(line):
                problems.append(f"{name}:{number}: section '{current}' must start with a number, e.g. '## 2.1 Daily checks'")
            if current in sections:
                problems.append(f"{name}:{number}: duplicate section '{current}'")
            sections[current] = []
        elif current is not None:
            sections[current].append(line)

        if EMAIL.search(line):
            problems.append(f"{name}:{number}: contains an email address")
        for match in LONG_NUMBER.finditer(line):
            if sum(ch.isdigit() for ch in match.group()) >= 10:
                problems.append(f"{name}:{number}: contains a phone or account number")

    if not sections:
        problems.append(f"{name}: no '## ' sections, so nothing can be searched")
    for heading, body in sections.items():
        content = " ".join(part.strip() for part in body if part.strip())
        if not content:
            problems.append(f"{name}: section '{heading}' is empty")
        elif len(content) > MAX_SECTION_CHARS:
            problems.append(f"{name}: section '{heading}' is {len(content)} characters (max {MAX_SECTION_CHARS}); split it")

    return problems


def call_api(base: str, path: str, token: str) -> dict:
    """POST with the supervisor token and return the JSON body, with plain-English errors."""
    request = urllib.request.Request(base.rstrip("/") + path, data=b"", method="POST",
                                     headers={"Authorization": f"Bearer {token}"})
    try:
        with urllib.request.urlopen(request, timeout=300) as response:
            return json.load(response)
    except urllib.error.HTTPError as error:
        reasons = {401: "token missing or expired", 403: "this needs a supervisor token"}
        sys.exit(f"API said {error.code}: {reasons.get(error.code, error.reason)}")
    except urllib.error.URLError:
        sys.exit(f"Can't reach the API at {base}. Is it running?")


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Check, publish and index FieldOps manuals.")
    parser.add_argument("source", type=Path, help="folder with the new .md manuals")
    parser.add_argument("--audience", choices=["all", "supervisor"], default="all")
    parser.add_argument("--check-only", action="store_true", help="only run the checks")
    parser.add_argument("--api", default="http://localhost:5266")
    parser.add_argument("--manuals", type=Path, default=DEFAULT_MANUALS)
    parser.add_argument("--min-hit3", type=float, default=0.8,
                        help="eval gate: share of eval questions whose right section is in the top 3")
    args = parser.parse_args(argv)

    files = sorted(args.source.glob("*.md"))
    if not files:
        print(f"No .md files in {args.source}")
        return 1

    # 1. Check everything first: publish nothing unless every file passes
    problems = [p for f in files for p in check_manual(f.name, f.read_text(encoding="utf-8"))]
    if problems:
        print("✗ Checks failed, nothing was published:")
        for problem in problems:
            print("  -", problem)
        return 1
    print(f"✓ {len(files)} manual(s) passed the checks")
    if args.check_only:
        return 0

    token = os.environ.get("FIELDOPS_TOKEN")
    if not token:
        print("Set FIELDOPS_TOKEN to a supervisor token first (see the top of this file).")
        return 1

    # 2. Publish: copy into the API's Manuals folder (supervisor/ = supervisor-only)
    target = args.manuals / "supervisor" if args.audience == "supervisor" else args.manuals
    target.mkdir(parents=True, exist_ok=True)
    for f in files:
        status = "updated" if (target / f.name).exists() else "new"
        shutil.copy2(f, target / f.name)
        print(f"  {status:7} {target.relative_to(args.manuals.parent) / f.name}")

    # 3. Re-index: the API re-reads the folder, embeds every section and replaces old chunks
    ingest = call_api(args.api, "/api/manuals/ingest", token)
    print(f"✓ Indexed {ingest['files']} files, {ingest['chunks']} sections")

    # 4. Gate: new content must not make search worse for the questions we already answer
    report = call_api(args.api, "/api/eval/retrieval", token)
    hit3 = report["hit3"] / report["cases"]
    print(f"Eval: Hit@1 {report['hit1']}/{report['cases']} · Hit@3 {report['hit3']}/{report['cases']} ({hit3:.0%})")
    if hit3 < args.min_hit3:
        print(f"✗ Hit@3 is below {args.min_hit3:.0%}: check which questions now miss before you commit")
        return 2
    print("✓ Eval gate passed")
    return 0


if __name__ == "__main__":
    sys.exit(main())