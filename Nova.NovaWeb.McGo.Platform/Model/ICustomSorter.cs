using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Platform.Model
{
    public interface ICustomSorter:IComparer
    {
        ListSortDirection SortDirection { get; set; }
    }
}
