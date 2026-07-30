import io
import tempfile
import unittest
from contextlib import redirect_stdout
from pathlib import Path

from ian import Runtime


class IanTests(unittest.TestCase):
    def run_code(self, code: str) -> str:
        with tempfile.TemporaryDirectory() as tmp:
            out = io.StringIO()
            with redirect_stdout(out):
                Runtime(Path(tmp)).run(code)
            return out.getvalue()

    def test_variables_and_sum(self):
        output = self.run_code('guardar x = "hola"\ndecir $x\nsumar 2 3 como total\ndecir $total\n')
        self.assertIn("hola", output)
        self.assertIn("5.0", output)

    def test_repeat(self):
        output = self.run_code('repetir 2 veces\n  decir "ok"\nfin\n')
        self.assertEqual(output.count("ok"), 2)

    def test_web_generation(self):
        with tempfile.TemporaryDirectory() as tmp:
            base = Path(tmp)
            Runtime(base).run(
                'web iniciar "Demo" como p\n'
                'web titulo p "Hola"\n'
                'web guardar p en "out/index.html"\n'
            )
            self.assertTrue((base / "out" / "index.html").exists())

    def test_agent_plan(self):
        output = self.run_code(
            'agente plan "objetivo" como p\n'
            'agente paso p hacer "uno"\n'
            'agente json p como salida\n'
            'decir $salida\n'
        )
        self.assertIn("objetivo", output)
        self.assertIn("uno", output)


if __name__ == "__main__":
    unittest.main()
