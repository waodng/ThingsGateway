﻿namespace ThingsGateway.Web.Foundation;
public static class AlarmHostServiceHelpers
{

    /// <summary>
    /// 获取bool报警类型
    /// </summary>
    public static AlarmEnum GetBoolAlarmCode(CollectVariableRunTime tag, out string limit, out string expressions)
    {
        limit = string.Empty;
        expressions = string.Empty;
        if (tag.BoolCloseAlarmEnable && tag.Value.ToBoolean() == false)
        {
            limit = false.ToString();
            expressions = tag.BoolCloseRestrainExpressions;
            return AlarmEnum.Close;
        }
        if (tag.BoolOpenAlarmEnable && tag.Value.ToBoolean() == true)
        {
            limit = true.ToString();
            expressions = tag.BoolOpenRestrainExpressions;
            return AlarmEnum.Open;
        }
        return AlarmEnum.None;
    }

    /// <summary>
    /// 获取value报警类型
    /// </summary>
    public static AlarmEnum GetDecimalAlarmDegree(CollectVariableRunTime tag, out string limit, out string expressions)
    {
        limit = string.Empty;
        expressions = string.Empty;

        if (tag.HHAlarmEnable && tag.Value.ToDecimal() > tag.HHAlarmCode.ToDecimal())
        {
            limit = tag.HHAlarmCode.ToString();
            expressions = tag.HHRestrainExpressions;
            return AlarmEnum.HH;
        }

        if (tag.HAlarmEnable && tag.Value.ToDecimal() > tag.HAlarmCode.ToDecimal())
        {
            limit = tag.HAlarmCode.ToString();
            expressions = tag.HRestrainExpressions;
            return AlarmEnum.H;
        }

        if (tag.LAlarmEnable && tag.Value.ToDecimal() < tag.LAlarmCode.ToDecimal())
        {
            limit = tag.LAlarmCode.ToString();
            expressions = tag.LRestrainExpressions;
            return AlarmEnum.L;
        }
        if (tag.LLAlarmEnable && tag.Value.ToDecimal() < tag.LLAlarmCode.ToDecimal())
        {
            limit = tag.LLAlarmCode.ToString();
            expressions = tag.LLRestrainExpressions;
            return AlarmEnum.LL;
        }
        return AlarmEnum.None;
    }
}