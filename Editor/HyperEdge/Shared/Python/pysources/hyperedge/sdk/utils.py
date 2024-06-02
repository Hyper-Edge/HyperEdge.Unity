import inflection
import re


def to_underscore(s: str):
    s = re.sub(r'[^\w\s]', '', s)
    return '_'.join([ss.lower() for ss in s.split()])


def camelize_string(s):
    return inflection.camelize('_'.join(s.split()))


def apply_ident(s, n):
    lines = s.split('\n')
    ident = ' ' * n
    return '\n'.join([ident + line for line in lines]) + '\n'



