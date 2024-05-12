using ECCore.Attributes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ECCore.Components
{

	public abstract partial class Component<TSelf> : IComponent
		where TSelf : Component<TSelf>
	{

		/// <summary>
		/// Does this component require the entity to be owned in order to run?
		/// </summary>
		public static OnSignalAttribute[] RegisteredSignals = typeof(TSelf)
			.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
			.Select(method => (method, attribute: method.GetCustomAttribute(typeof(OnSignalAttribute)) as OnSignalAttribute))
			.Where(method => method.attribute != null)
			.Select(method => {
				// Check the parameters
				foreach (var parameter in method.method.GetParameters())
				{
					if (parameter.ParameterType.IsSubclassOf(typeof(Signal<>).MakeGenericType(parameter.ParameterType)))
					{
						method.attribute.acceptedSignalType = parameter.ParameterType;
					}
					else
					{
						throw new Exception($"An OnSignal function was registered with improper arguments. It must have only a single argument representing the signal to be registering.\n" +
							$"If the parameter is in fact a signal, make sure it derives from the Signal<> class otherwise it cannot not be handled nor networked.");
					}
				}
				var genericMethod = typeof(TSelf)
					.GetMethod("Register", BindingFlags.NonPublic | BindingFlags.Instance)
					.MakeGenericMethod(method.attribute.acceptedSignalType);
				// Set the registration action
				method.attribute.registrationAction = (target) =>
                {
                    Delegate raiser = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(method.attribute.acceptedSignalType), target, method.method);
                    genericMethod.Invoke(target, new object[] { raiser, method.attribute.AcceptFrom, method.attribute.RunOn });
                };
				return method.attribute;
			})
			.ToArray();
	}
}
