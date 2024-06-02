import os
import pathlib

from hyperedge.sdk.codegen import *
from hyperedge.sdk.dataloader import DataLoader
from hyperedge.sdk.unipipe import send_to_unity
from hyperedge.sdk.client import HEClient, AppDefDTO
from hyperedge.sdk.appdata import AppData, AppEnvData


class HeApp:
    def __init__(self, app_name):
        self._app_name = app_name
        self._client = HEClient()
        self._app_manifest = AppData.load()

    @property
    def slug(self):
        return self._app_name

    def collect(self):
        dl = DataLoader(self._app_name)
        return dl.to_dict()

    def _load_appdef(self) -> AppDefDTO:
        j_app_data = self.collect()
        app_def = AppDefDTO(Name=self._app_manifest.Name, **j_app_data)
        if self._app_manifest.Id:
            app_def.Id = self._app_manifest.Id
        return app_def

    def export_current(self, do_gencode: bool):
        j_app_data = self.collect()
        app_def = AppDefDTO(Name=self._app_manifest.Name, **j_app_data)
        #
        resp = self._client.export_gen_code_current_version(self._app_manifest.Id, app_def, do_gencode)
        #
        curr_path = pathlib.Path(os.environ['HE_ASSETS_PATH']).joinpath('AppVersions', 'current.json')
        if not curr_path.parent.exists():
            os.makedirs(str(curr_path.parent), exist_ok=True)
        self._client.download_file_by_id(resp['AppDefFileId'], str(curr_path))
        #
        send_to_unity('PyCollect', dict(AppId=resp['AppId'], AppName=self._app_manifest.Name))

    def release(self, version_name: str):
        if not self._app_manifest.Id:
            raise Exception("AppId is empty. You should export app first")
        if self._app_manifest.has_version(version_name):
            print(f"Version {version_name} already exist")
            return
        app_def = self._load_appdef()
        resp = self._client.release_app(app_def, self._app_manifest.Id, version_name)
        #
        send_to_unity('PyRelease', resp.dict())

    def build_app_version(self, version_name: str, do_build: bool):
        if not self._app_manifest.has_version(version_name):
            raise Exception(f"Unknown version: {version_name}")
        resp = self._client.build_app_version(
            self._app_manifest.Id, version_name, do_build)
        #
        send_to_unity('PyBuild', resp.dict())

    def export(self):
        app_def = self._load_appdef()
        resp = self._client.export_app(app_def)
        ret = resp.dict()
        ret['Name'] = self._app_manifest.Name
        #
        send_to_unity('PyExport', ret)

    def run(self, version_name: str, env_name: str):
        version = self._app_manifest.get_version(version_name)
        if version is None:
            print(f"Version {version_name} doesn't exist.")
            return
        app_env = self._app_manifest.get_app_env(env_name)
        if app_env is None:
            print(f"AppEnv {env_name} doesn't exist.")
            return
        resp = self._client.run_app(self._app_manifest.Id, version.Id, app_env.Id)
        #
        send_to_unity('PyRun', {})

    def codegen(self):
        app_def = self._load_appdef()
        cgen = ServerMockGen(app_def)
        cgen.generate()
        #
        send_to_unity('PyCodegen', {})

    def generate_client_code(self):
        app_def = self._load_appdef()
        cgen = ClientCodeGen(app_def)
        cgen.generate()
        #
        send_to_unity('PyClientCodeGen', {})

    def llm_gen_data(self, data_cls_name):
        resp = self._client.llm_gen_data(self._app_manifest.Id, data_cls_name)
        #
        send_to_unity('PyLlmGenData', resp.dict())

    def llm_propose_model(self, entity_name: str, description: str):
        resp = self._client.llm_propose_model(self._app_manifest.Id, entity_name, description)
        send_to_unity('PyLlmProposeModel', resp.dict())

    def convert_llm_thread_to_app_def(self, ai_thread_id: str):
        resp = self._client.convert_llm_thread_to_app_def(self._app_manifest.Id, ai_thread_id)
        send_to_unity('PyConvertLlmThreadToAppDef', resp.dict())

    def get_models_paths(self):
        return pathlib.Path(os.environ['HE_APP_PATH']).joinpath(self._app_name, 'models')

