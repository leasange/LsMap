using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsMap.Workspace
{
    /// <summary>
    /// 工作空间连接管理类
    /// </summary>
    public class WorkspaceConnection
    {
        private WorkspaceType _wstype = WorkspaceType.None;
        private string _wsFileName = null;

        internal List<LsMap.Map.Map> _maps = new List<LsMap.Map.Map>();//地图集合
        internal List<LsMap.Data.Datasource> _datasources = new List<LsMap.Data.Datasource>();//数据源集合

        public WorkspaceConnection(string wsFileName, string user, string password)
        {
            _wstype = WorkspaceType.File;
            _wsFileName = wsFileName;
        }

        internal bool Open()
        {
            switch (_wstype)
            {
                case WorkspaceType.None:
                    break;
                case WorkspaceType.File:
                    Workspace ws = Workspace.OpenFileWorkspace(_wsFileName);
                    _maps = ws.Maps;
                    _datasources = ws.Datasources;
                    break;
                default:
                    break;
            }
            return true;
        }
    }

    public enum WorkspaceType
    {
        None,
        File,
    }
}
