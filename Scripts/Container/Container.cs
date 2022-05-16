using Godot;
using System.Reflection;

namespace GodotModules
{
    /// A very basic Singleton DI Container
    public class Container
    {
        private static Dictionary<Type, object> _container = new();

        public static T Get<T>() =>
            (T)Get(typeof(T));

        private static object Get(Type type)
        {
            if (!_container.ContainsKey(type))
                throw new KeyNotFoundException($"Type {type.Name} not found in container");

            return _container[type];
        }

        public static T Register<T>(T value)
        {
            _container[typeof(T)] = value;
            return value;
        }

        public static T Register<T>() =>
            InjectAndRegister(Activator.CreateInstance<T>());

        public static T InjectAndRegister<T>(T value)
        {
            Inject(value);
            Register<T>(value);
            return value;
        }

        public static T Inject<T>(T value)
        {
            Inject((object)value);
            return value;
        }

        private static void Inject(object value)
        {
            var type = value.GetType();

            var fields = type
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.GetCustomAttributes(typeof(InjectAttribute), true).Any());

            foreach (var field in fields)
                field.SetValue(value, Get(field.FieldType));

            var methods = type
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes(typeof(AfterInjectAttribute), true).Any());

            foreach (var method in methods)
                method.Invoke(value, null);
        }
    }
}