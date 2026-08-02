using System;
using System.Data;
using Dapper;
using GameWatch.Core.Dto;

namespace GameWatch.Core;

public static class DapperHelpers
{
    public class GameIdTypeHandler : SqlMapper.TypeHandler<GameId>
    {
        public override void SetValue(IDbDataParameter parameter, GameId value) => parameter.Value = value.V;
        public override GameId Parse(object value) => new (Convert.ToInt32(value));
    }

    public class GameIdxTypeHandler : SqlMapper.TypeHandler<GameIdx>
    {
        public override void SetValue(IDbDataParameter parameter, GameIdx value) => parameter.Value = value.V;
        public override GameIdx Parse(object value) => new (Convert.ToInt32(value));
    }

    public class PidTypeHandler : SqlMapper.TypeHandler<Pid>
    {
        public override void SetValue(IDbDataParameter parameter, Pid value) => parameter.Value = value.V;
        public override Pid Parse(object value) => new (Convert.ToInt32(value));
    }

    public class ElapsedTimeTypeHandler : SqlMapper.TypeHandler<ElapsedTime>
    {
        public override void SetValue(IDbDataParameter parameter, ElapsedTime value) => parameter.Value = value.V;
        public override ElapsedTime Parse(object value) => new (Convert.ToInt32(value));
    }
}