import unittest

from ingest import check_manual

GOOD = """# CV-10 Conveyor: Service Notes

Sample manual.

## 1.1 Safety
Apply lockout/tagout before touching the belt.

## 2.1 Belt tension
Belt sag must be 15 to 20 mm at mid-span. Checked on 2026-10-03 for part CP-200.
"""


class CheckManualTests(unittest.TestCase):
    def test_good_manual_passes(self):
        self.assertEqual(check_manual("good.md", GOOD), [])

    def test_missing_title(self):
        problems = check_manual("x.md", GOOD.replace("# CV-10 Conveyor: Service Notes", "CV-10 Conveyor"))
        self.assertTrue(any("title" in p for p in problems))

    def test_no_sections(self):
        problems = check_manual("x.md", "# Title\n\nJust text.\n")
        self.assertTrue(any("no '## ' sections" in p for p in problems))

    def test_empty_section(self):
        problems = check_manual("x.md", GOOD + "\n## 3.1 Tracking\n\n")
        self.assertTrue(any("is empty" in p for p in problems))

    def test_duplicate_section(self):
        problems = check_manual("x.md", GOOD + "\n## 2.1 Belt tension\nAgain.\n")
        self.assertTrue(any("duplicate" in p for p in problems))

    def test_unnumbered_section(self):
        problems = check_manual("x.md", GOOD + "\n## Tracking\nKeep the belt centred.\n")
        self.assertTrue(any("must start with a number" in p for p in problems))

    def test_email_is_blocked(self):
        problems = check_manual("x.md", GOOD + "Questions: ravi.k@example.com\n")
        self.assertTrue(any("email" in p for p in problems))

    def test_phone_is_blocked(self):
        problems = check_manual("x.md", GOOD + "Call +91 98765 43210\n")
        self.assertTrue(any("phone" in p for p in problems))

    def test_dates_and_part_numbers_are_not_pii(self):
        self.assertEqual(check_manual("x.md", GOOD + "Next service 2026-12-01, part SK-200.\n"), [])

    def test_huge_section(self):
        problems = check_manual("x.md", GOOD + "\n## 3.1 Long\n" + "word " * 400 + "\n")
        self.assertTrue(any("split it" in p for p in problems))


if __name__ == "__main__":
    unittest.main()