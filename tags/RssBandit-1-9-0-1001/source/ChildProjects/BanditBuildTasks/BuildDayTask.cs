using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Org.RssBandit.MSBuild
{
	/// <summary>
    /// Task generates an numeric Build-day output
    /// </summary>
    /// <example>
    /// <para>Generates a common version file.</para>
    /// <code><![CDATA[
	/// <BuildDay>
	///   <Output TaskParameter="BuildDayOfYear" PropertyName="Build" />
	/// </BuildDay>
    /// ]]></code>
    /// <para>Fill the property named 'Build' with a number like this:
    /// 8210 ('8' is the current year, '11' would be returned for 2011;
    /// 210 is the day of the current year).</para>
    public sealed class BuildDay : Task
	{
		// Fields
		private string buildDay = "";

		/// <summary>
		/// When overridden in a derived class, executes the task.
		/// </summary>
		/// <returns>
		/// true if the task successfully executed; otherwise, false.
		/// </returns>
		public override bool Execute()
		{
			this.buildDay = string.Format("{0}{1:000}", DateTime.Now.Year % 10, DateTime.Now.DayOfYear);
			return true;
		}

		/// <summary>
		/// Gets the build day of the current year.
		/// </summary>
		/// <value>The build day.</value>
		[Output]
		public string BuildDayOfYear
		{
			get
			{
				return this.buildDay;
			}
		}
	}


}
