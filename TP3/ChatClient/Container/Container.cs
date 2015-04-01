namespace MVVM.Container
{
    public static class Container
    {
        private static IContainer _containerImplementation;

        public static void InitializeContainerWith(IContainer containerImplementation)
        {
            _containerImplementation = containerImplementation;
        }

        public static T GetA<T>()
        {
            return _containerImplementation.GetA<T>();
        }
    }
}