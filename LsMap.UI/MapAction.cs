using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LsMap.UI
{
    public class MapAction
    {
        public static readonly MapAction Default;
        public static readonly MapAction Move;
        

        private Cursor _upCursor=Cursors.Default;
        public Cursor upCursor
        {
            get { return _upCursor; }
        }
        private Cursor _downCursor = Cursors.Default;
        public Cursor downCursor
        {
            get { return _downCursor; }
        }
        static MapAction()
        {
            Default = new MapAction(Cursors.Default, Cursors.Default);
            Move = new MapAction(Cursors.Hand,Cursors.Hand);
        }

        public MapAction(Cursor upcursor,Cursor downcursor)
        {
            _upCursor = upcursor;
            _downCursor = downcursor;
        }

    }
}
