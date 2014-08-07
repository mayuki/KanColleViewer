using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper;

namespace Misuzilla.KanColleViewer.Plugins.AzureMobileServicesNotifier
{
    /// <summary>
    /// なんか拡張メソッド類
    /// </summary>
    internal static class Extension
    {
        /// <summary>
        /// IDisposableの列挙をまとめてCompositeDisposableに追加します
        /// </summary>
        /// <param name="disposables"></param>
        /// <param name="compositeDisposable"></param>
        /// <returns></returns>
        public static IEnumerable<IDisposable> AddTo(this IEnumerable<IDisposable> disposables, CompositeDisposable compositeDisposable)
        {
            foreach (var disposable in disposables)
            {
                compositeDisposable.Add(disposable);
            }
            return disposables;
        }
        /// <summary>
        /// IDisposableの列挙をCompositeDisposableに追加します
        /// </summary>
        /// <param name="disposables"></param>
        /// <param name="compositeDisposable"></param>
        /// <returns></returns>
        public static IDisposable AddTo(this IDisposable disposable, CompositeDisposable compositeDisposable)
        {
            compositeDisposable.Add(disposable);
            return disposable;
        }

        /// <summary>
        /// INotifyPropertyChangedをObservableに変換します
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IObservable<PropertyChangedEventArgs> FromNotifyPropertyChanged<T>(this T target) where T : INotifyPropertyChanged
        {
            // AppDomainを超えられなくて死ぬっぽい
            return Observable.Create<PropertyChangedEventArgs>(observer =>
            {
                PropertyChangedEventHandler func = (sender, e) =>
                {
                    observer.OnNext(e);
                };
                target.PropertyChanged += func;
                return () => target.PropertyChanged -= func;
            });
            //return Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
            //    h => target.PropertyChanged += h,
            //    h => target.PropertyChanged -= h
            //);
        }

        /// <summary>
        /// 電の本気を見るのです。
        /// </summary>
        public static void FireAndForget(this Task task)
        {
            task.ContinueWith(x =>
                              {
                                  Trace.WriteLine("FireAndForget(Exception): " + x.Exception);
                              }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
