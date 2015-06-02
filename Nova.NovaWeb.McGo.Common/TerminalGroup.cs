using Nova.NovaWeb.Common;
/*
 * 由SharpDevelop创建。
 * 用户： Lixc
 * 日期: 2013/9/2
 * 时间: 14:55
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Collections.Generic;

namespace Nova.NovaWeb.McGo.Common
{
	/// <summary>
	/// Description of TerminalGroup.
	/// </summary>
	public class TerminalGroup
	{
        private Group _group;
		public TerminalGroup()
		{
		}

        public TerminalGroup(Group group)
        {
            _group = group;
        }
		
		public int Id { get {return _group.GroupID;}}

        public string Name { get { return _group.GroupName; } }

        public Group CurrentGroup
        {
            get
            {
                return _group;
            }
        }

        public List<TerminalModel> Terminals
        {
            get
            {
                var terminalList = new List<TerminalModel>();
                _group.SiteList.ForEach(s => terminalList.Add(new TerminalModel(_group,s,new SiteStatus())));
                return terminalList;
            }
        }		
		
	}
}
