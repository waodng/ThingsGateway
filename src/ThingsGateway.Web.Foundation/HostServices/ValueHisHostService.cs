﻿using Furion.Logging.Extensions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 实时数据库后台服务
/// </summary>
public class ValueHisHostService : BackgroundService, ISingleton
{
    private readonly ILogger<ValueHisHostService> _logger;
    private GlobalCollectDeviceData _globalCollectDeviceData;
    private IntelligentConcurrentQueue<CollectVariableRunTime> CollectDeviceVariables { get; set; } = new(50000);
    private IntelligentConcurrentQueue<CollectVariableRunTime> ChangeDeviceVariables { get; set; } = new(50000);
    public OperResult StatuString { get; set; } = new OperResult("初始化");



    private static IServiceScopeFactory _scopeFactory;
    public ValueHisHostService(ILogger<ValueHisHostService> logger, IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _globalCollectDeviceData = scopeFactory.CreateScope().ServiceProvider.GetService<GlobalCollectDeviceData>();
    }
    public async Task<OperResult<SqlSugarClient>> HisConfig()
    {
        await Task.CompletedTask;
        using var serviceScope = _scopeFactory.CreateScope();
        var ConfigService = serviceScope.ServiceProvider.GetService<IConfigService>();
        var hisEnable = (await ConfigService.GetByConfigKey(ThingsGatewayConst.ThingGateway_HisConfig_Base, ThingsGatewayConst.Config_His_Enable))?.ConfigValue?.ToBoolean();
        var hisDbType = (await ConfigService.GetByConfigKey(ThingsGatewayConst.ThingGateway_HisConfig_Base, ThingsGatewayConst.Config_His_DbType))?.ConfigValue;
        var hisConnstr = (await ConfigService.GetByConfigKey(ThingsGatewayConst.ThingGateway_HisConfig_Base, ThingsGatewayConst.Config_His_ConnStr))?.ConfigValue;

        if (!(hisEnable == true))
        {
            return new OperResult<SqlSugarClient>("历史数据已配置为Disable");
        }

        var configureExternalServices = new ConfigureExternalServices
        {
            EntityService = (type, column) => // 修改列可空-1、带?问号 2、String类型若没有Required
            {
                if ((type.PropertyType.IsGenericType && type.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    || (type.PropertyType == typeof(string) && type.GetCustomAttribute<RequiredAttribute>() == null))
                    column.IsNullable = true;
            },
        };

        DbType type = DbType.SqlServer;
        if (!string.IsNullOrEmpty(hisDbType))
        {
            if (Enum.TryParse<DbType>(hisDbType, ignoreCase: true, out var result))
            {
                type = result;
            }
            else
            {
                return new OperResult<SqlSugarClient>("数据库类型转换失败");
            }
        }

        var sqlSugarClient = new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = hisConnstr,//连接字符串
            DbType = type,//数据库类型
            IsAutoCloseConnection = true, //不设成true要手动close
            ConfigureExternalServices = configureExternalServices,
        });
        return OperResult.CreateSuccessResult(sqlSugarClient);
    }
    #region worker服务
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("历史服务启动");
        await base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("历史服务停止");
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(60000, stoppingToken);
            }
            catch (TaskCanceledException)
            {

            }
        }
    }


    #endregion

    #region core
    /// <summary>
    /// 循环线程取消标识
    /// </summary>
    public ConcurrentList<CancellationTokenSource> StoppingTokens = new();
    private Task<Task> ValueHisTask;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        ValueHisTask = new Task<Task>(() =>
        {
            CancellationTokenSource StoppingToken = StoppingTokens.Last();
            return Task.Factory.StartNew(async (a) =>
            {
                _logger?.LogInformation($"历史数据线程开始");

                try
                {

                    var result = await HisConfig();
                    if (!result.IsSuccess)
                    {
                        _logger?.LogWarning($"历史数据线程即将退出：" + result.Message);
                        StatuString = new OperResult($"已退出：{result.Message}");
                        return;
                    }
                    else
                    {
                        var sqlSugarClient = result.Content;
                        bool LastIsSuccess = true;
                        /***创建/更新单个表***/
                        try
                        {
                            await sqlSugarClient.Queryable<ValueHis>().FirstAsync();
                        }
                        catch (Exception)
                        {
                            try
                            {
                                sqlSugarClient.CodeFirst.InitTables(typeof(ValueHis));
                            }
                            catch (Exception)
                            {
                            }
                        }

                        while (!StoppingToken.Token.IsCancellationRequested)
                        {
                            try
                            {
                                await Task.Delay(500, StoppingToken.Token);
                                try
                                {
                                    await sqlSugarClient.Queryable<ValueHis>().FirstAsync();
                                }
                                catch (Exception)
                                {
                                    sqlSugarClient.CodeFirst.InitTables(typeof(ValueHis));
                                    throw new("数据库测试连接失败");
                                }
                                LastIsSuccess = true;
                                StatuString = OperResult.CreateSuccessResult();
                                if (StoppingToken.Token.IsCancellationRequested)
                                    break;
                                //这里直接出队，没做失败重试，后续添加
                                var list = CollectDeviceVariables.ToListWithDequeue(CollectDeviceVariables.Count);
                                var changelist = ChangeDeviceVariables.ToListWithDequeue(ChangeDeviceVariables.Count);
                                await sqlSugarClient.Queryable<ValueHis>().FirstAsync();
                                if (list.Count != 0)
                                {
                                    ////Sql保存
                                    var collecthis = list.Adapt<List<ValueHis>>();
                                    //插入
                                    await sqlSugarClient.Insertable<ValueHis>(collecthis).ExecuteCommandAsync();
                                }

                                if (changelist.Count != 0)
                                {
                                    ////Sql保存
                                    var changehis = changelist.Adapt<List<ValueHis>>();
                                    //插入
                                    await sqlSugarClient.Insertable<ValueHis>(changehis).ExecuteCommandAsync();

                                }


                            }
                            catch (TaskCanceledException)
                            {

                            }
                            catch (Exception ex)
                            {
                                if (LastIsSuccess)
                                    _logger?.LogError(ex, $"历史数据循环异常");
                                StatuString = new OperResult($"异常：请查看后台日志");
                                LastIsSuccess = false;
                            }
                        }

                    }
                }
                catch (TaskCanceledException)
                {

                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"历史数据循环异常");
                }
            }, StoppingToken.Token
 , TaskCreationOptions.LongRunning);

        }
 );
    }

    internal void Stop(IEnumerable<CollectDeviceRunTime> devices = null)
    {

        foreach (var device in devices)
        {
            device.DeviceVariableRunTimes?.ForEach(v => { v.VariableCollectChange -= DeviceVariableCollectChange; });
            device.DeviceVariableRunTimes?.ForEach(v => { v.VariableValueChange -= DeviceVariableValueChange; });
        }

        CancellationTokenSource StoppingToken = StoppingTokens.Last();
        StoppingToken?.Cancel();

        _logger?.LogInformation($"历史数据线程停止中");
        var hisHisResult = ValueHisTask?.Result;
        if (hisHisResult?.Status != TaskStatus.Canceled)
        {
            if (hisHisResult?.Wait(5000) == true)
            {
                _logger?.LogInformation($"历史数据线程已停止");
            }
            else
            {
                _logger?.LogInformation($"历史数据线程停止超时，已强制取消");
            }
        }
        StoppingTokens.Remove(StoppingToken);
        ValueHisTask?.Dispose();

    }
    internal void Start()
    {
        foreach (var device in _globalCollectDeviceData.CollectDevices)
        {
            device.DeviceVariableRunTimes?.ForEach(v => { v.VariableCollectChange += DeviceVariableCollectChange; });
            device.DeviceVariableRunTimes?.ForEach(v => { v.VariableValueChange += DeviceVariableValueChange; });
        }
        StoppingTokens.Add(new());
        Init();
        ValueHisTask.Start();

    }

    public void Restart()
    {
        Stop(_globalCollectDeviceData.CollectDevices);
        Start();
    }




    private void DeviceVariableCollectChange(CollectVariableRunTime variable)
    {
        if (variable.HisType == HisType.Collect)
        {
            CollectDeviceVariables.Enqueue(variable);
        }
    }
    private void DeviceVariableValueChange(CollectVariableRunTime variable)
    {
        if (variable.HisType == HisType.Change)
        {
            ChangeDeviceVariables.Enqueue(variable);
        }
    }




    #endregion
}
public class ValueHisMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<CollectVariableRunTime, ValueHis>()
            .Map(dest => dest.Value, (src) => ValueReturn(src));
    }

    private static object ValueReturn(CollectVariableRunTime src)
    {
        if (src.DataType == typeof(bool))
        {
            if (src.Value == null || src.Value?.ToString()?.ToUpper() == "FALSE" || src.Value?.ToString()?.ToUpper() == "0")
            {
                return 0;
            }
            else { return 1; }
        }
        else
        {
            return src.Value;
        }
    }
}

