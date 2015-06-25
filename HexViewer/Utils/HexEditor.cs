using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace HexViewer.Utils
{
    [Designer(typeof(HexEditorDesigner))]
    public partial class HexEditor : Control
    {
        private bool _test = true;

        [EditorBrowsable(EditorBrowsableState.Always), DefaultValue(true), RefreshProperties(RefreshProperties.Repaint)]
        public bool Test { get { return _test; } set { _test = value; } }

        public HexEditor()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.DrawRectangle(new Pen(Color.Red, 2), new Rectangle(1, 1, Width - 2, Height - 2));
            base.OnPaint(pe);
        }
    }

    public class HexEditorDesignerActionList : DesignerActionList
    {
        private readonly HexEditor _hexEditor;
        private readonly DesignerActionUIService _designerActionUiService;

        public bool Something
        {
            get { return _hexEditor.Test; }
            set
            {
                _hexEditor.Test = value;
                _designerActionUiService.Refresh(Component);
            }
        }

        public HexEditorDesignerActionList(IComponent component) : base(component)
        {
            _hexEditor = component as HexEditor;
            _designerActionUiService = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }
    }

    public class HexEditorDesigner : ControlDesigner
    {
        private DesignerActionListCollection _actionLists;

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (_actionLists == null)
                {
                    _actionLists = new DesignerActionListCollection {new HexEditorDesignerActionList(Component)};
                }

                return _actionLists;
            }
        }
    }
}
