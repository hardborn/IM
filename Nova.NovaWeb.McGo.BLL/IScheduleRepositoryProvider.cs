using Nova.NovaWeb.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.BLL
{
    public interface IScheduleRepositoryProvider
    {
        IEnumerable<Schedule> FindAll();

        void Update(Schedule schedule);

        IEnumerable<Schedule> FilterBy(System.Linq.Expressions.Expression<Func<Schedule, object>> expression);
    }
}
