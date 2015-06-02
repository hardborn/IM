using Nova.NovaWeb.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Common
{
    [Serializable]
    public class Account
    {
        public Account()
        {
        }

        public Account(string name, string password)
        {
            Name = name;
            Password = password;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }


    }
}
