using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;

using HyperEdge.Shared.Protocol.Models;
using HyperEdge.Shared.Protocol.Models.Export;


namespace HyperEdge.Sdk.Unity
{
    public class AppDef
    {
	    private readonly AppDefDTO _appDef;
        public AppDefDTO Data { get => _appDef; }

	    private Dictionary<Ulid, DataClassDTO> _DataClassesById = new();
	    private Dictionary<string, DataClassDTO> _DataClassesByName = new();
	    private Dictionary<string, DataClassDTO> _DataClassesByModelName = new();

	    public IReadOnlyDictionary<Ulid, DataClassDTO> DataClassesById { get => _DataClassesById; }
	    public IReadOnlyDictionary<string, DataClassDTO> DataClassesByName { get => _DataClassesByName; }

	    public IEnumerable<DataClassDTO> DataClasses { get => _appDef.DataClasses; }

	    public AppDef(AppDefDTO appDef)
	    {
	        this._appDef = appDef;
	        foreach (var dataCls in _appDef.DataClasses)
            {
                _DataClassesById[dataCls.Id] = dataCls;
		        _DataClassesByName[dataCls.Name] = dataCls;
            }
            foreach (var modelCls in _appDef.ModelClasses)
            {
                foreach(var fld in modelCls.Fields)
                {
                    if (_DataClassesByName.ContainsKey(fld.Typename))
                    {
                        _DataClassesByModelName[modelCls.Name] = _DataClassesByName[fld.Typename];
                        break;
                    }
                }
            }
        }

	    public List<DataClassInstanceDTO>? GetDataClassInstancesByModelName(string name)
        {
            if (_DataClassesByModelName.TryGetValue(name, out var dataCls))
            {
                return GetDataClassInstancesByName(dataCls.Name);
            }
            return null;
        }

	    public List<DataClassInstanceDTO>? GetDataClassInstancesByName(string name)
        {
            if (_appDef.DataClassInstances.TryGetValue(name, out var instances))
            {
                return instances;
            }
            return null;
        }
    }
}

