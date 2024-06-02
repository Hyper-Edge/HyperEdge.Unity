import os
import sys
import typer
sys.path.append(os.environ['HE_SYS_PATH'])

from hyperedge.sdk.cli import cli_app
from hyperedge.sdk.he_app import HeApp


@cli_app.callback()
def setup_he_app(ctx: typer.Context):
    ctx.obj = HeApp('{{ cookiecutter.project_slug }}')


if __name__ == "__main__":
    cli_app()

