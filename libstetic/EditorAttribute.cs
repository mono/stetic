using System;

namespace Stetic {

	[AttributeUsage (AttributeTargets.Property)]
	public sealed class EditorAttribute : Attribute {

		public EditorAttribute (Type editorType)
		{
			this.editorType = editorType;
		}

		Type editorType;
		public Type EditorType {
			get { return editorType; }
			set { editorType = value; }
		}

		int editorSize = -1;
		public int EditorSize {
			get { return editorSize; }
			set { editorSize = value; }
		}
	}
}
