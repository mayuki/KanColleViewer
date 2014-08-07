using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;

using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Misuzilla.KanColleViewer.Plugins.AzureMobileServicesNotifier.MobileServices;

namespace Misuzilla.KanColleViewer.Plugins.AzureMobileServicesNotifier
{
    [Export(typeof(INotifier))]
    public class AzureMobileServicesNotifier : INotifier
    {
        private CompositeDisposable _disposables;
        private AzureMobileServicesNotification _notification;

        public void Initialize()
        {
            // Azure Mobile Servicesのクライアントを保存されていればそれから作る
            _notification = new AzureMobileServicesNotification();
            _notification.InitializeClientFromSavedData();

            KanColleClient.Current.FromNotifyPropertyChanged()
                .SkipWhile(x => !KanColleClient.Current.IsStarted)
                .Take(1)
                .Subscribe(_ => InitializeObservables());
        }

        public void Show(NotifyType type, string header, string body, Action activated, Action<Exception> failed = null)
        {
        }

        public object GetSettingsView()
        {
            return null;
        }

        public void Dispose()
        {
            if (_disposables != null)
            {
                _disposables.Dispose();
            }
        }

        private void InitializeObservables()
        {
            if (_disposables != null)
            {
                _disposables.Dispose();
            }
            _disposables = new CompositeDisposable();

            // 建造
            KanColleClient.Current.Homeport.Dockyard.FromNotifyPropertyChanged()
                .Where(x => x.PropertyName == "Docks")
                .Subscribe(_ => InitializeObservables())
                .AddTo(_disposables);
            KanColleClient.Current.Homeport.Dockyard.Docks.Values
                .Select(dock =>
                {
                    return dock.FromNotifyPropertyChanged()
                        .Where(x => x.PropertyName == "State" || x.PropertyName == "CompleteTime")
                        .Subscribe(_ =>
                        {
                            Debug.WriteLine(String.Format("BuildingDock Changed: Id={0}, State={1}, Ship={2}", dock.Id, dock.State, dock.Ship));
                            if (dock.State == BuildingDockState.Building)
                            {
                                _notification.Upsert(AzureMobileNotificationType.Construction,
                                                    dock.Id,
                                                    "???"/*dock.Ship.Name*/,
                                                    dock.CompleteTime)
                                             .FireAndForget();
                            }
                            else
                            {
                                _notification.Delete(AzureMobileNotificationType.Construction, dock.Id)
                                             .FireAndForget();
                            }
                        });
                })
                .AddTo(_disposables);

            // 入渠
            KanColleClient.Current.Homeport.Repairyard.FromNotifyPropertyChanged()
                .Where(x => x.PropertyName == "Docks")
                .Subscribe(_ => InitializeObservables())
                .AddTo(_disposables);
            KanColleClient.Current.Homeport.Repairyard.Docks.Values
                .Select(dock =>
                {
                    return dock.FromNotifyPropertyChanged()
                        .Where(x => x.PropertyName == "State" || x.PropertyName == "Ship" || x.PropertyName == "CompleteTime")
                        .Subscribe(_ =>
                                   {
                                        var name = (dock.Ship != null && dock.Ship.Info != null) ? dock.Ship.Info.Name : "-";
                                        Debug.WriteLine(String.Format("RepairingDock Changed: Id={0}, State={1}, Ship={2}", dock.Id, dock.State, name));
                                        if (dock.State == RepairingDockState.Repairing)
                                        {
                                            _notification.Upsert(AzureMobileNotificationType.Repair,
                                                                dock.Id,
                                                                name,
                                                                dock.CompleteTime)
                                                         .FireAndForget();
                                        }
                                        else
                                        {
                                            _notification.Delete(AzureMobileNotificationType.Repair, dock.Id)
                                                         .FireAndForget();
                                        }
                                   });
                })
                .AddTo(_disposables);

            // 遠征
            KanColleClient.Current.Homeport.Organization.FromNotifyPropertyChanged()
                .Where(x => x.PropertyName == "Fleets")
                .Subscribe(_ => InitializeObservables())
                .AddTo(_disposables);
            KanColleClient.Current.Homeport.Organization.Fleets.Values
                .Select(fleet =>
                {
                    return fleet.Expedition.FromNotifyPropertyChanged()
                            .Where(x => x.PropertyName == "IsInExecution" || x.PropertyName == "ReturnTime")
                            .Subscribe(_ =>
                                       {
                                            var title = (fleet.Expedition.Mission != null) ? fleet.Expedition.Mission.Title : "-";
                                            Debug.WriteLine(String.Format("Expedition Changed: FleetId={0}, IsInExecution={1}, Title={2}, Fleet={3}", fleet.Id, fleet.Expedition.IsInExecution, title, fleet.Name));
                                            if (fleet.Expedition.IsInExecution)
                                            {
                                                _notification.Upsert(AzureMobileNotificationType.Expeditions,
                                                                    fleet.Id,
                                                                    String.Format("{0}({1})", title, fleet.Name),
                                                                    fleet.Expedition.ReturnTime)
                                                             .FireAndForget();
                                            }
                                            else
                                            {
                                                _notification.Delete(AzureMobileNotificationType.Expeditions, fleet.Id)
                                                             .FireAndForget();
                                            }
                                       });
                })
                .AddTo(_disposables);
        }
    }
}
