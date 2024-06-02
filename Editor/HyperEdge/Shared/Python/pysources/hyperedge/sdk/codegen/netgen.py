import jinja2
import os
import pathlib

from hyperedge.sdk.client import AppDefDTO
from hyperedge.sdk.utils import apply_ident
from hyperedge.sdk.codegen.base import CodeGenBase


class NetCodeGen(CodeGenBase):
    def __init__(self, mp_name: str, app_def: AppDefDTO):
        super().__init__(app_def)
        self._mp_name = mp_name
    
    def generate(self):
        self._gen_asmdef()

    def _gen_asmdef(self):
        t = self._templ_env.get_template('multiplayer/asmdef.tmpl')
        code_s = t.render(namespace=self._namespace)
        self.write_file(pathlib.Path(self._output_path).joinpath(f'{self._namespace}.{self._mp_name}.asmdef'), code_s)

