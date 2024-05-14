using Assets.Code.Networking.Communication.ApplicationLayer;
using Assets.Code.Networking.Communication.NetworkLayer;
using Assets.Code.Networking.Communication.Packets;
using Assets.Code.Networking.Serialisation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ECCore.Components
{
    public abstract partial class Component<TSelf> : IComponent
        where TSelf : Component<TSelf>
    {

        public uint NetworkID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">We need to know who the target is so that we know if we need to serialise the entity.</param>
        /// <returns></returns>
        public void Serialise(INetworkInterface target, BinaryWriter writer)
        {
            foreach (var property in ComponentCache<TSelf>.SerialisedProperties)
            {
                SerialisationHelper.Serialise(target, property.PropertyType, property.GetValue(this), writer);
            }
            foreach (var field in ComponentCache<TSelf>.SerialisedFields)
            {
                SerialisationHelper.Serialise(target, field.FieldType, field.GetValue(this), writer);
            }
        }

        public void Deserialise(INetworkInterface sender, INetworkObjectTracker localObjects, BinaryReader reader)
        {
            foreach (var property in ComponentCache<TSelf>.SerialisedProperties)
            {
                var value = SerialisationHelper.Deserialise(sender, localObjects, property.PropertyType, reader);
                property.SetValue(this, value);
            }
            foreach (var field in ComponentCache<TSelf>.SerialisedFields)
            {
                var value = SerialisationHelper.Deserialise(sender, localObjects, field.FieldType, reader);
                field.SetValue(this, value);
            }
        }

    }

    public static class ComponentCache<TComponentType>
            where TComponentType : Component<TComponentType>
    {

        public static IEnumerable<PropertyInfo> SerialisedProperties = typeof(TComponentType)
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => (!x.DeclaringType.IsConstructedGenericType || x.DeclaringType.GetGenericTypeDefinition() != typeof(Component<>)) && x.GetCustomAttribute(typeof(NetIgnoreAttribute)) == null);

        public static IEnumerable<FieldInfo> SerialisedFields = typeof(TComponentType)
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => (!x.DeclaringType.IsConstructedGenericType || x.DeclaringType.GetGenericTypeDefinition() != typeof(Component<>)) && x.GetCustomAttribute(typeof(NetIgnoreAttribute)) == null);

    }

}
