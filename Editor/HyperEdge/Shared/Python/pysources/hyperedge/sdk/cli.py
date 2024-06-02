import inflection
import os
import pathlib
import typer
from typing import List, Optional
import json 

from hyperedge.sdk.dataloader import DataLoader
from hyperedge.sdk.unipipe import send_to_unity
from hyperedge.sdk.client import HEClient, AppDefDTO
from hyperedge.sdk.appdata import AppData, AppEnvData


cli_app = typer.Typer()


@cli_app.command()
def collect(ctx: typer.Context):
    """
    Collects all data model definitions in the project, and prints them out in JSON format.
    """
    ctx.obj.export_current(False)


@cli_app.command()
def codegen(ctx: typer.Context):
    """
    Generates some code locally
    """
    ctx.obj.codegen()


@cli_app.command()
def generate_client_code(ctx: typer.Context):
    """
    Generate UnityClient code locally
    """
    ctx.obj.generate_client_code()


@cli_app.command()
def export(ctx: typer.Context):
    """
    Exports all data model definitions in the project to hyperedge's backend
    """
    ctx.obj.export()


@cli_app.command()
def release(ctx: typer.Context, version_name: str):
    """
    Make a release and push all data model definitions in the project to hyperedge's backend
    """
    ctx.obj.release(version_name)


@cli_app.command()
def build_app_version(ctx: typer.Context, version_name: str):
    """
    Build docker container for released version
    """
    ctx.obj.build_app_version(version_name, do_build=True)


@cli_app.command()
def gen_code_app_version(ctx: typer.Context, version_name: str):
    """
    Build docker container for released version
    """
    ctx.obj.build_app_version(version_name, do_build=False)


@cli_app.command()
def create_env(ctx: typer.Context, env_name: str):
    app_manifest = AppData.load()
    if app_manifest.has_app_env(env_name):
        print(f'AppEnv {env_name} already exists.')
        return
    client = HEClient()
    resp = client.create_app_env(app_manifest.Id, env_name)
    app_manifest.add_app_env(AppEnvData(Id=resp.Id, Name=resp.Name))
    app_manifest.save()


@cli_app.command()
def run(ctx: typer.Context, version_name: str, env_name: str):
    ctx.obj.run(version_name, env_name)


@cli_app.command()
def export_gen_code_current_version(ctx: typer.Context):
    ctx.obj.export_current(True)


@cli_app.command()
def export_build_run_current_version(ctx: typer.Context, do_build: bool, do_run: bool):
    app_manifest = AppData.load()
    j_app_data = ctx.obj.collect()
    app_def = AppDefDTO(Name=app_manifest.Name, **j_app_data)
    client = HEClient()
    resp = client.export_build_run_current_version(app_manifest.Id, app_def, do_build, do_run)


@cli_app.command()
def stop(ctx: typer.Context, version_name: str, env_name: str):
    app_manifest = AppData.load()
    version = app_manifest.get_version(version_name)
    if version is None:
        print(f"Version {version_name} doesn't exist.")
        return
    app_env = app_manifest.get_app_env(env_name)
    if app_env is None:
        print(f"AppEnv {env_name} doesn't exist.")
        return
    client = HEClient()
    resp = client.stop_app(app_manifest.Id, version.Id, app_env.Id)


@cli_app.command()
def gen_code(ctx: typer.Context):
    app_manifest = AppData.load()
    client = HEClient()
    resp = client.gen_code(app_manifest.Id)
    print(resp)


@cli_app.command()
def start_server(ctx: typer.Context):
    app_manifest = AppData.load()
    client = HEClient()
    resp = client.start_server(app_manifest.Id)
    print(resp)


@cli_app.command()
def llm_gen_data(ctx: typer.Context, data_cls_name: str):
    ctx.obj.llm_gen_data(data_cls_name)


@cli_app.command()
def llm_propose_model(ctx: typer.Context, entity_name: str, description: str):
    ctx.obj.llm_propose_model(entity_name, description)


@cli_app.command()
def convert_llm_thread_to_app_def(ctx: typer.Context, ai_thread_id: str):
    ctx.obj.convert_llm_thread_to_app_def(ai_thread_id)


def _write_py_file(p, s):
    if p.exists():
        raise Exception(f"File {str(p)} already exists")
    #
    if not p.parent.exists():
        os.makedirs(str(p.parent), exist_ok=True)
    #
    with open(str(p), 'w') as f:
        f.write(s)


def _output_fields(fields):
    s = ""
    fields = fields.split(',')
    if not fields:
        s += "    pass\n"
    for fld in fields:
        fld_ = fld.split(':')
        fname, ftype = fld_
        s += f"    {inflection.camelize(fname)}: {ftype}\n"
    return s


@cli_app.command()
def create_dataclass(ctx: typer.Context,
                  name: str = typer.Argument(..., help="Name of the Dataclass"), 
                  fields: Optional[str] = typer.Option(..., "--fields", help='Custom Fields')):
    """
    Create a new data class, provided with <name> and <fields>. Fields should be seperated by space and each field is in the <name>:<type> attribute format.
    """
    fname = inflection.underscore(name)
    fpath = ctx.obj.get_models_paths().joinpath('data', f'{fname}.py')
    #
    s = f"""from hyperedge.sdk.models import BaseData, DataRef
from hyperedge.sdk.models.types import *


class {inflection.camelize(name)}Data(BaseData):
"""
    if not fields:
        fields = "Name:str"
    #
    s += _output_fields(fields)
    #
    _write_py_file(fpath, s)
    ctx.obj.export_current(False)


@cli_app.command()
def create_struct(ctx: typer.Context, 
                  name: str = typer.Argument(..., help="Name of the Struct"), 
                  fields: Optional[str] = typer.Option(..., "--fields", help='Custom Fields')):
    """
    Create a new struct class, provided with <name> and <fields>. Fields should be seperated by space and each field is in the <name>:<type> attribute format.
    """
    fname = inflection.underscore(name)
    fpath = ctx.obj.get_models_paths().joinpath('data', f'{fname}.py')
    st_name = inflection.camelize(name)
    #
    s = f"""from hyperedge.sdk.models.base import _BaseModel
from hyperedge.sdk.models.types import *


class {st_name}(_BaseModel):
"""
    s += _output_fields(fields)
    #
    _write_py_file(fpath, s)
    ctx.obj.export_current(False)


@cli_app.command()
def create_event(ctx: typer.Context,
                 name: str = typer.Argument(..., help="Name of the Event Class"), 
                 fields: Optional[str] = typer.Option(..., "--fields", help='Custom Fields')):
    """
    Create a new event class, provided with <name> and <fields>. Fields should be seperated by space and each field is in the <name>:<type> attribute format.
    """
    fname = inflection.underscore(name)
    fpath = ctx.obj.get_models_paths().joinpath('data', f'{fname}.py')
    st_name = inflection.camelize(name)
    #
    s = f"""from hyperedge.sdk.models import BaseEvent
from hyperedge.sdk.models.types import *


class {st_name}(BaseEvent):
"""
    s += _output_fields(fields)
    #
    _write_py_file(fpath, s)
    ctx.obj.export_current(False)


@cli_app.command()
def create_model(ctx: typer.Context,
                 name: str = typer.Argument(..., help="Name of the Model"),
                 data_fields: Optional[str] = typer.Option(..., "--data-fields", help='Custom static Data fields'),
                 model_fields: Optional[str] = typer.Option(..., "--model-fields", help='Custom Model fields')):
    """
    Create a new data model, provided with <name>, <data_fields>, and <model_fields>. Fields should be seperated by space and each field is in the <name>:<type> attribute format.
    """
    fname = inflection.underscore(name)
    fpath = ctx.obj.get_models_paths().joinpath(f'{fname}.py')
    #
    entity_name = inflection.camelize(name)
    #
    s = """from hyperedge.sdk.models import DataModel, BaseData, DataRef
"""
    if data_fields:
        s += f"""
class {entity_name}Data(BaseData):
"""
        s += _output_fields(data_fields)
    #
    s += f"""

class {entity_name}(DataModel):
"""
    s += _output_fields(model_fields)
    if data_fields:
        s += f"""
    Data: DataRef[{entity_name}Data]
"""
    #
    _write_py_file(fpath, s)
    ctx.obj.export_current(False)


@cli_app.command()
def create_storage(ctx: typer.Context,
                   name: str = typer.Argument(..., help="Name of the Storage"),
                   storage_type: Optional[str] = typer.Option(..., "--storage-type", help='Storage type'),
                   model_fields: Optional[str] = typer.Option(..., "--fields", help='Custom Storage fields')):

    fname = inflection.underscore(name)
    fpath = ctx.obj.get_models_paths().joinpath(f'{fname}.py')
    #
    entity_name = inflection.camelize(name)
    #
    s = """
from hyperedge.sdk.models.storage import *
from hyperedge.sdk.models.types import *

"""
    #
    s += f"""
class {entity_name}({storage_type}):
"""
    s += _output_fields(model_fields)
    #
    _write_py_file(fpath, s)
    ctx.obj.export_current(False)


@cli_app.command()
def create_user_group(ctx: typer.Context,
                      name: str = typer.Argument(..., help="Name of the Model"),
                      fields: Optional[str] = typer.Option(None, "--fields", help='Custom Fields')):
    """
    Create a new UserGroup data model, provided with <name>, and <fields>. Fields should be seperated by space and each field is in the <name>:<type> attribute format.
    """
    fname = inflection.underscore(name)
    fpath = ctx.obj.get_models_paths().joinpath(f'{fname}.py')
    #
    s = f"""
from hyperedge.sdk.models import UserGroup
from hyperedge.sdk.models.types import *


class {inflection.camelize(name)}(UserGroup):
"""
    s += _output_fields(fields)
    #
    _write_py_file(fpath, s)
    ctx.obj.export_current(False)


@cli_app.command()
def create_energy_system(ctx: typer.Context,
                         name: str = typer.Argument(..., help="Name of the Energy System"),
			 data_fields: Optional[str] = typer.Option(..., '--data-fields', help="Custom fields for energy system static data"),
			 model_fields: Optional[str] = typer.Option(..., '--model-fields', help="Custom fields for energy system model")):
    """
    Create energy system
    """
    fname = inflection.underscore(name)
    fpath = ctx.obj.get_models_paths().joinpath(f'{fname}.py')

    es_name = inflection.camelize(name)
    #
    s = f"""
from hyperedge.sdk.models.energy_system import EnergySystem
from hyperedge.sdk.models import BaseData, DataRef
from hyperedge.sdk.models import DataModel

"""
    #
    if data_fields:
        s += f"""
class {es_name}EnergySystemData(BaseData):
"""
        s += _output_fields(data_fields)
    #
    if model_fields:
        s += f"""
class {es_name}EnergySystemModel(DataModel):
"""
        s += _output_fields(model_fields)
    #
    s += f"{es_name} = EnergySystem(Name=\"{es_name}\""
    if data_fields:
        s += f", Data={es_name}EnergySystemData"
    if model_fields:
        s += f", Model={es_name}EnergySystemModel"
    s += ")"
    #
    _write_py_file(fpath, s)
    ctx.obj.export_current(False)


@cli_app.command()
def create_reward(ctx: typer.Context,
                  name: str = typer.Argument(..., help="Name of the Reward"), 
                  fields: Optional[List[str]] = typer.Option(None, '--fields', help='Custom Fields')):
    """
    Create a reward.
    """
    fname = inflection.underscore(name)
    fpath = ctx.obj.get_models_paths().joinpath(f'{fname}.py')
    #
    s = f"""from { ctx.obj.slug }.models.sdk.models import DataModel

class Reward{inflection.camelize(name)}(DataModel):
    ItemId: str
    Amount: int
"""
    for fld in fields:
        fld_ = fld.split(':')
        fname, ftype = fld_
        s += f"    {inflection.camelize(fname)}: {ftype}\n"

    
    if fpath.exists():
        raise Exception(f"File {str(fpath)} already exists")

    with open(str(fpath), 'w') as f:
        f.write(s)


@cli_app.command()
def create_cost(ctx: typer.Context,
                name: str = typer.Argument(..., help="Name of the Cost"), 
                fields: Optional[List[str]] = typer.Option(None, '--fields', help='Custom Fields')):
    """
    Create a cost.
    """
    fname = inflection.underscore(name)
    fpath = ctx.obj.get_models_paths().joinpath(f'{fname}.py')
    #
    s = f"""from { ctx.obj.slug }.models.sdk.models import DataModel

class Cost{inflection.camelize(name)}(DataModel):
    ItemId: str
    Amount: int
"""
    for fld in fields:
        fld_ = fld.split(':')
        fname, ftype = fld_
        s += f"    {inflection.camelize(fname)}: {ftype}\n"

    
    if fpath.exists():
        pass

    with open(str(fpath), 'w') as f:
        f.write(s)


@cli_app.command()
def create_craft(ctx: typer.Context,
                 name: str = typer.Argument(..., help="Name of the Craft"), 
                 fields: Optional[List[str]] = typer.Option(None, '--fields', help='Custom Fields')):
    """
    Create a craft.
    """
    fname = inflection.underscore(name)
    fpath = ctx.obj.get_models_paths().joinpath(f'{fname}.py')
    #
    s = f"""from { ctx.obj.slug }.models.sdk.models import DataModel, Dataref

class Cost{inflection.camelize(name)}(DataModel):
    Cost: Dataref[str]
    Reward: Dataref[str]
"""
    for fld in fields:
        fld_ = fld.split(':')
        fname, ftype = fld_
        s += f"    {inflection.camelize(fname)}: {ftype}\n"

    if fpath.exists():
        raise Exception(f"File {str(fpath)} already exists")

    with open(str(fpath), 'w') as f:
        f.write(s)


@cli_app.command()
def create_quest(ctx: typer.Context,
                 name: str = typer.Argument(..., help="Name of the Craft"), 
                 fields: Optional[List[str]] = typer.Option(None, '--fields', help='Custom Fields')):
    """
    Create a quest.
    """
    fname = inflection.underscore(name)
    fpath = ctx.obj.get_models_paths().joinpath(f'{fname}.py')
    #
    s = f"""from { ctx.obj.slug }.models.sdk.models import DataModel, str

class Quest{inflection.camelize(name)}(DataModel):
    AcceptConditions: str
    FinishConditions: str
    Reward: DataRef[str]
"""
    for fld in fields:
        fld_ = fld.split(':')
        fname, ftype = fld_
        s += f"    {inflection.camelize(fname)}: {ftype}\n"

    if fpath.exists():
        raise Exception(f"File {str(fpath)} already exists")
    
    with open(str(fpath), 'w') as f:
        f.write(s)


@cli_app.command()
def create_ladder(ctx: typer.Context,
                  entity_name: str = typer.Argument(..., help="Entity name for the Ladder"), 
                  is_exp_based: Optional[bool] = typer.Option(False, '--is-exp-based', is_flag=True, help='Is experience based'),
                  level_data_fields: Optional[str] = typer.Option(None, '--level-data-fields', help='Custom Fields')):
    """
    Create a ladder.
    """
    fname = inflection.underscore(entity_name) + '_ladder'
    fpath = ctx.obj.get_models_paths().joinpath(f'{fname}.py')
    entity = inflection.camelize(entity_name)
    #
    s = f"""#
from hyperedge.sdk.models import BaseData, ProgressionLadder
from hyperedge.sdk.models.types import *

from { ctx.obj.slug }.models.{inflection.underscore(entity_name)} import {entity}


class {entity}LadderLevelData(BaseData):
"""
    s += _output_fields(level_data_fields)
    if not level_data_fields:
        s += "    pass\n"
    #
    s += f"""

{entity}Ladder = ProgressionLadder(
    Entity={entity},
    IsExperienceBased={bool(is_exp_based)},
    LadderLevelData={entity}LadderLevelData)
"""
    #
    _write_py_file(fpath, s)
    ctx.obj.export_current(False)


@cli_app.command()
def create_battlepass(ctx: typer.Context,
                  name: str = typer.Argument(..., help="Name of the BattlePass"), 
                  data_fields: Optional[str] = typer.Option(None, '--data-fields', help='Custom Fields'),
                  level_data_fields: Optional[str] = typer.Option(None, '--level-data-fields', help='Custom Fields'),
                  model_fields: Optional[str] = typer.Option(None, '--model-fields', help='Custom Fields')):
    """
    Create a BattlePass.
    """
    fname = inflection.underscore(name)
    fpath = ctx.obj.get_models_paths().joinpath(f'{fname}.py')
    #
    bp_name = inflection.camelize(name)
    #
    s = f"""
from hyperedge.sdk.models.battle_pass import BattlePass
from hyperedge.sdk.models import BaseData, DataRef
from hyperedge.sdk.models import DataModel

"""
    #
    if data_fields:
        s += f"""
class {bp_name}BattlePassData(BaseData):
"""
        s += _output_fields(data_fields)

    if level_data_fields:
        s += f"""
class {bp_name}BattlePassLevelData(BaseData):
"""
        s += _output_fields(level_data_fields)
    #
    if model_fields:
        s += f"""
class {bp_name}BattlePassModel(DataModel):
"""
        s += _output_fields(model_fields)
    #
    s += f"{bp_name} = BattlePass(Name=\"{bp_name}\""
    if data_fields:
        s += f", Data={bp_name}BattlePassData"
    #
    if level_data_fields:
        s += f", LevelData={bp_name}BattlePassLevelData"
    #
    if model_fields:
        s += f", Model={bp_name}BattlePassModel"
    s += ")"
    #
    _write_py_file(fpath, s)
    ctx.obj.export_current(False)


@cli_app.command()
def create_tournament(
                  ctx: typer.Context,
                  name: str = typer.Argument(..., help="Name of the BattlePass"), 
                  data_fields: Optional[str] = typer.Option(None, '--data-fields', help='Custom Fields'),
                  model_fields: Optional[str] = typer.Option(None, '--model-fields', help='Custom Fields')):
    """
    Create a Tournament.
    """
    fname = inflection.underscore(name)
    fpath = ctx.obj.get_models_paths().joinpath(f'{fname}.py')
    #
    bp_name = inflection.camelize(name)
    #
    s = f"""
from hyperedge.sdk.models import Tournament
from hyperedge.sdk.models import BaseData, DataRef
from hyperedge.sdk.models import DataModel

"""
    #
    if data_fields:
        s += f"""
class {bp_name}TournamentData(BaseData):
"""
        s += _output_fields(data_fields)
    #
    if model_fields:
        s += f"""
class {bp_name}TournamentModel(DataModel):
"""
        s += _output_fields(model_fields)
    #
    s += f"{bp_name} = Tournament(Name=\"{bp_name}\""
    #
    if data_fields:
        s += f", Data={bp_name}TournamentData"
    #
    if model_fields:
        s += f", Model={bp_name}TournamentModel"
    s += ")"
    #
    _write_py_file(fpath, s)
    ctx.obj.export_current(False)


@cli_app.command()
def create_req_handler(
                  ctx: typer.Context,
                  name: str = typer.Argument(..., help="Name of the RequestHandler"), 
                  req_fields: Optional[str] = typer.Option(None, '--req-fields', help='Custom Fields'),
                  resp_fields: Optional[str] = typer.Option(None, '--resp-fields', help='Custom Fields')):
    """
    Create a RequestHandler.
    """
    fname = inflection.underscore(name)
    fpath = ctx.obj.get_models_paths().joinpath('logic', f'{fname}.py')
    #
    rh_name = inflection.camelize(name)
    #
    hcs_path = pathlib.Path(os.environ['HE_APP_PATH']).joinpath('server_scripts', 'Handlers', f'{rh_name}.hcs')
    s = f"""
return new {rh_name}Response();
"""
    _write_py_file(hcs_path, s)
    #
    s = f"""#
from hyperedge.sdk.models.base import _BaseModel
from hyperedge.sdk.models.handler import Handler
from hyperedge.sdk.models.types import *

"""
    #
    s += f"""
class {rh_name}Request(_BaseModel):
"""
    s += _output_fields(req_fields)
    #
    s += f"""
class {rh_name}Response(_BaseModel):
"""
    s += _output_fields(resp_fields)
    #
    s += f"""
{rh_name}Handler = Handler(
    Name='{rh_name}',
    RequestClass={rh_name}Request,
    ResponseClass={rh_name}Response)
"""
    #
    _write_py_file(fpath, s)
    ctx.obj.export_current(False)


@cli_app.command()
def create_job_handler(
        ctx: typer.Context,
        name: str = typer.Argument(..., help="Name of the JobHandler"),
        jd_fields: Optional[str] = typer.Option(None, '--job-data-fields', help='Custom Fields')):
 
    fname = inflection.underscore(name)
    fpath = ctx.obj.get_models_paths().joinpath('logic', 'jobs', f'{fname}.py')
    #
    jh_name = inflection.camelize(name)
    #
    hcs_path = pathlib.Path(os.environ['HE_APP_PATH']).joinpath('server_scripts', 'JobHandlers', f'{jh_name}.hcs')
    _write_py_file(hcs_path, "")
    #
    s = f"""#
from hyperedge.sdk.models.base import _BaseModel
from hyperedge.sdk.models.job import Job
from hyperedge.sdk.models.types import *

"""
    #
    s += f"""
class {jh_name}JobRequest(_BaseModel):
"""
    s += _output_fields(jd_fields)
    #
    s += f"""
{jh_name}JobHandler = Job(
    Name='{jh_name}',
    JobDataClass={jh_name}JobRequest)
"""
    #
    #
    _write_py_file(fpath, s)
    ctx.obj.export_current(False)

