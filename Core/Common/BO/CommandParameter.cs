using System.Collections.Generic;
using System.Data.SqlClient;

namespace PLM.DBA.Common.BO
{
	public class CommandParameter
	{
		public string Command { get; private set; }
		public IEnumerable<SqlParameter> Parameters { get; private set; }
		public object NewId { get; set; }
        public object Count { get; set; }
		public object[] OutParameterValue { get; set; }

		public CommandParameter(string cmd, IEnumerable<SqlParameter> parameters)
		{
			Command = cmd;
			Parameters = parameters;
		}

		public CommandParameter(string cmd)
		{
			Command = cmd;
			Parameters = null;
		}

	}
}
