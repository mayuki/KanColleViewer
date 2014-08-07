using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misuzilla.KanColleViewer.Plugins.AzureMobileServicesNotifier.MobileServices
{
    public class AzureMobileNotificationItem
    {
        public AzureMobileNotificationItem(String userId, AzureMobileNotificationType type, Int32 id, String description, DateTime completionTimeUtc)
        {
            UserId = userId;
            Type = type;
            Id = id;
            Description = description;
            CompletionTimeUtc = completionTimeUtc;
        }

        public AzureMobileNotificationItem()
        {
        }

        /// <summary>
        /// ユーザーID
        /// </summary>
        public String UserId { get; set; }

        /// <summary>
        /// 通知の種類
        /// </summary>
        public AzureMobileNotificationType Type { get; set; }

        /// <summary>
        /// ID (要するに番号)
        /// </summary>
        public Int32 Id { get; set; }

        /// <summary>
        /// 説明
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// 完了時間
        /// </summary>
        public DateTime CompletionTimeUtc { get; set; }
    }

    public enum AzureMobileNotificationType
    {
        /// <summary>
        /// 入渠
        /// </summary>
        Repair = 1,
        /// <summary>
        /// 建造
        /// </summary>
        Construction = 2,
        /// <summary>
        /// 遠征
        /// </summary>
        Expeditions = 3
    }
}
