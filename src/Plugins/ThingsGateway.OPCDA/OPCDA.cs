﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OpcDaClient.Da;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Adapter.OPCDA;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.OPCDA
{
    public class OPCDAClient : DriverBase
    {
        internal CollectDeviceRunTime Device;
        private List<CollectVariableRunTime> _deviceVariables = new();
        internal ThingsGateway.Foundation.Adapter.OPCDA.OPCDAClient PLC = null;
        public override System.Type DriverUI => typeof(ImportVariable);

        public OPCDAClient(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
        }
        [DeviceProperty("IP", "")] public string OPCIP { get; set; } = "localhost";
        [DeviceProperty("OPC名称", "")] public string OPCName { get; set; } = "Kepware.KEPServerEX.V6";
        [DeviceProperty("激活订阅", "")] public bool ActiveSubscribe { get; set; } = true;
        [DeviceProperty("更新频率", "")] public int UpdateRate { get; set; } = 1000;
        [DeviceProperty("检测重连频率", "")] public int CheckRate { get; set; } = 60000;
        [DeviceProperty("死区", "")] public float DeadBand { get; set; } = 0;
        [DeviceProperty("自动分组大小", "")] public int GroupSize { get; set; } = 500;

        public override ThingsGatewayBitConverter ThingsGatewayBitConverter { get; } = new(EndianType.Little);
        public override void AfterStop()
        {
            PLC?.Disconnect();
        }

        public override async Task BeforStart()
        {
            PLC.Connect();
            await Task.CompletedTask;
        }

        public override void Dispose()
        {
            PLC.DataChangedHandler -= dataChangedHandler;
            PLC.Disconnect();
            PLC.Dispose();
            PLC = null;
        }


        public override bool IsSupportAddressRequest()
        {
            return !ActiveSubscribe;
        }
        public override OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(List<CollectVariableRunTime> deviceVariables)
        {
            _deviceVariables = deviceVariables;
            if (deviceVariables.Count > 0)
            {
                var result = PLC.SetTags(deviceVariables.Select(a => a.VariableAddress).ToList());
                var sourVars = result?.Select(
          it =>
          {
              return new DeviceVariableSourceRead(UpdateRate)
              {
                  Address = it.Key,
                  DeviceVariables = deviceVariables.Where(a => it.Value.Select(b => b.ItemID).Contains(a.VariableAddress)).ToList()
              };
          }).ToList();
                return OperResult.CreateSuccessResult(sourVars);
            }
            else
            {
                return OperResult.CreateSuccessResult(new List<DeviceVariableSourceRead>());
            }
        }

        public override async Task<OperResult<byte[]>> ReadSourceAsync(DeviceVariableSourceRead deviceVariableSourceRead, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var result = PLC.ReadSub(deviceVariableSourceRead.Address);
            return result.Copy<byte[]>();
        }

        public override async Task<OperResult> WriteValueAsync(CollectVariableRunTime deviceVariable, string value)
        {
            await Task.CompletedTask;
            var result = PLC.Write(deviceVariable.VariableAddress, value);
            return result;
        }

        protected override void Init(CollectDeviceRunTime device, object client = null)
        {
            Device = device;
            OPCNode oPCNode = new();
            oPCNode.OPCIP = OPCIP;
            oPCNode.OPCName = OPCName;
            oPCNode.UpdateRate = UpdateRate;
            oPCNode.DeadBand = DeadBand;
            oPCNode.GroupSize = GroupSize;
            oPCNode.ActiveSubscribe = ActiveSubscribe;
            oPCNode.CheckRate = CheckRate;
            if (PLC == null)
            {
                PLC = new(TouchSocketConfig.Container.Resolve<ILog>());
                PLC.DataChangedHandler += dataChangedHandler;
            }
            PLC.Init(oPCNode);
        }

        protected override Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken)
        {
            //不走ReadAsync
            throw new NotImplementedException();
        }
        private void dataChangedHandler(List<ItemReadResult> values)
        {
            try
            {
                if (!Device.Enable)
                {
                    return;
                }
                Device.DeviceStatus = DeviceStatusEnum.OnLine;

                if (IsLogOut)
                    _logger?.LogTrace(ToString() + " OPC值变化" + values.ToJson());

                foreach (var data in values)
                {
                    if (!Device.Enable)
                    {
                        return;
                    }

                    var itemReads = _deviceVariables.Where(it => it.VariableAddress == data.Name).ToList();
                    foreach (var item in itemReads)
                    {
                        var value = data.Value;
                        var quality = data.Quality;
                        var time = data.TimeStamp;
                        if (value != null && quality == 192)
                        {
                            item.SetValue(value);

                        }
                        else
                        {
                            item.SetValue(null);
                            Device.DeviceStatus = DeviceStatusEnum.OnLineButNoInitialValue;
                            Device.DeviceOffMsg = $"{item.Name} 质量为Bad ";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ToString());
                Device.DeviceStatus = DeviceStatusEnum.OnLineButNoInitialValue;
                Device.DeviceOffMsg = ex.Message;
            }
        }
    }
}
