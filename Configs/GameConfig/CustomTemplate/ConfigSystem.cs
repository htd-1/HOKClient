using System;
using Luban;
using GameConfig;
using UnityEngine;

/// <summary>
/// 配置加载器。不直接依赖任何框架模块，通过注入的 loader 委托加载 bytes。
/// 框架集成由 ConfigService（GameLogic 程序集）负责。
/// </summary>
public class ConfigSystem
{
    private static ConfigSystem _instance;

    public static ConfigSystem Instance => _instance ??= new ConfigSystem();

    private bool _init = false;

    private Tables _tables;
    private Func<string, ByteBuf> _loader;

    public Tables Tables
    {
        get
        {
            if (!_init)
            {
                Debug.LogWarning("[ConfigSystem] Tables accessed before Load completed.");
            }

            return _tables;
        }
    }

    public bool IsLoaded => _init;

    /// <summary>
    /// 设置 bytes 加载委托并同步加载所有配置表。
    /// </summary>
    /// <param name="loader">委托：传入文件名，返回 ByteBuf。由调用方负责资源加载和缓存。</param>
    public void Load(Func<string, ByteBuf> loader)
    {
        if (_init) return;
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));

        try
        {
            _tables = new Tables(_loader);
            _init = true;
            Debug.Log("[ConfigSystem] All config tables loaded successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ConfigSystem] Failed to load config tables: {e}");
            throw;
        }
    }

    /// <summary>
    /// 释放配置缓存。调用方负责卸载已加载的资源。
    /// </summary>
    public void Release()
    {
        if (!_init) return;
        _tables = null;
        _loader = null;
        _init = false;
        Debug.Log("[ConfigSystem] Config tables released.");
    }
}
