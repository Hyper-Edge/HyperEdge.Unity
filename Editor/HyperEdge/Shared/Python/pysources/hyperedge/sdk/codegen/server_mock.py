import jinja2
import os
import pathlib

from hyperedge.sdk.client import AppDefDTO
from hyperedge.sdk.utils import apply_ident
from hyperedge.sdk.codegen.base import CodeGenBase


class ServerMockGen(CodeGenBase):
    def __init__(self, app_def: AppDefDTO):
        super().__init__(app_def)

    def generate(self):
        self._gen_game_service()
        self._gen_req_handlers()
        self._gen_asmdef()

    def _gen_asmdef(self):
        t = self._templ_env.get_template('asmdef.tmpl')
        code_s = t.render(namespace=self._namespace)
        self.write_file(pathlib.Path(self._output_path).joinpath(f'{self._namespace}.ServerMock.asmdef'), code_s)

    def _gen_game_service(self):
        t = self._templ_env.get_template('game_service.cs.tmpl')
        code_s = t.render(namespace=self._namespace)
        self.write_file(pathlib.Path(self._output_path).joinpath('GameService.cs'), code_s)

    def _gen_req_handlers(self):
        t = self._templ_env.get_template('req_handler.cs.tmpl')
        for rh_dto in self._app_def.RequestHandlers:
            code_s = t.render(
                namespace=self._namespace,
                req_typename=rh_dto.RequestClassName,
                resp_typename=rh_dto.ResponseClassName,
                req_handler_name=rh_dto.Name,
                req_handler_code=apply_ident(rh_dto.Code, 12))
            #
            self.write_file(pathlib.Path(self._output_path).joinpath('Handlers', f'{rh_dto.Name}.cs'), code_s)

