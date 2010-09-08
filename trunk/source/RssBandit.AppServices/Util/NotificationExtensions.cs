#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

// keep namespace, it is the same as the extendee.
namespace System.ComponentModel
{
  /// <summary>
  /// PropertyChanged helpers
  /// </summary>
  /// <remarks>
  /// From http://www.ingebrigtsen.info/post/2008/12/11/INotifyPropertyChanged-revisited.aspx
  /// </remarks>
  public static class NotificationExtensions
  {
    public static void Notify(this PropertyChangedEventHandler eventHandler, Expression<Func<object>> expression)
    {
      if (null == eventHandler)
      {
        return;
      }
      LambdaExpression lambda = expression;
      MemberExpression memberExpression;
      if (lambda.Body is UnaryExpression)
      {
        UnaryExpression unaryExpression = lambda.Body as UnaryExpression;
        memberExpression = unaryExpression.Operand as MemberExpression;
      }
      else
      {
        memberExpression = lambda.Body as MemberExpression;
      }
      ConstantExpression constantExpression = memberExpression.Expression as ConstantExpression;
      PropertyInfo propertyInfo = memberExpression.Member as PropertyInfo;

      eventHandler(constantExpression.Value, new PropertyChangedEventArgs(propertyInfo.Name));
    }
  }
}
;