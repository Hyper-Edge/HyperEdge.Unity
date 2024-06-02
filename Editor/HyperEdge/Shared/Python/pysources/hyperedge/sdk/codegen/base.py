import jinja2
import os
import pathlib

from hyperedge.sdk.client import AppDefDTO
from hyperedge.sdk.utils import apply_ident


class CodeGenBase(object):
    def __init__(self, app_def: AppDefDTO):
        self._app_def = app_def
        self._output_path = str(pathlib.Path(os.environ['HE_ASSETS_PATH']).joinpath('ServerAssemblies'))
        self._namespace = f'HyperEdge.App.{self._app_def.Name}'
        self._templ_path = pathlib.Path(__file__).parent.joinpath('templates')
        self._templ_loader = jinja2.FileSystemLoader(str(self._templ_path))
        self._templ_env = jinja2.Environment(loader=self._templ_loader)

    def write_file(self, path, s):
        if not pathlib.Path.exists(path.parent):
            os.makedirs(str(path.parent), exist_ok=True)
        with open(str(path), 'w') as f:
            f.write(s)
    
    def generate(self):
        pass

