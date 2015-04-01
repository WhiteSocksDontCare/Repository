using System;

namespace MVVM.Container
{
    public interface IContainer
    {
        T GetA<T>();
        void RegisterA<T>(Type type);
    }
}