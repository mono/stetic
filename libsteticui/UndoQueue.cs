
using System;
using System.Collections;

namespace Stetic
{
	public class UndoQueue
	{
		ArrayList changeList = new ArrayList ();
		int undoListCount = 0;
		static UndoQueue empty = new UndoQueue ();
		
		public void AddChange (UndoRedoChange change)
		{
			if (undoListCount < changeList.Count) {
				// Destroy all undone changes
				changeList.RemoveRange (undoListCount, changeList.Count - undoListCount);
			}
			changeList.Add (change);
			undoListCount = changeList.Count;
		}
		
		public static UndoQueue Empty {
			get { return empty; }
		}
		
		public bool CanUndo {
			get { return undoListCount > 0; }
		}
		
		public bool CanRedo {
			get { return undoListCount < changeList.Count; }
		}
		
		public void Undo ()
		{
			if (undoListCount == 0)
				return;

			UndoRedoChange change = (UndoRedoChange) changeList [--undoListCount];
			if (change.CheckValid ()) {
				object res = change.ApplyChange ();
				if (res != null)
					changeList [undoListCount] = res;
				else
					// Undo failed
					changeList.RemoveAt (undoListCount);
			} else {
				changeList.RemoveAt (undoListCount);
				Undo ();
			}
		}
		
		public void Redo ()
		{
			if (undoListCount == changeList.Count)
				return;
			
			UndoRedoChange change = (UndoRedoChange) changeList [undoListCount++];
			if (change.CheckValid ()) {
				object res = change.ApplyChange ();
				if (res != null)
					changeList [undoListCount - 1] = res;
				else {
					// Redo failed
					undoListCount--;
					changeList.RemoveAt (undoListCount);
				}
			}
			else {
				changeList.RemoveAt (--undoListCount);
				Redo ();
			}
		}
		
		public void Purge ()
		{
			for (int n=0; n<changeList.Count; n++) {
				UndoRedoChange change = (UndoRedoChange) changeList [n];
				if (!change.CheckValid()) {
					changeList.RemoveAt (n);
					if (n < undoListCount)
						undoListCount--;
				}
			}
		}
	}
	
	public abstract class UndoRedoChange: MarshalByRefObject
	{
		public abstract UndoRedoChange ApplyChange ();

		public virtual bool CheckValid ()
		{
			return true;
		}
	}
	
	class ObjectWrapperUndoRedoChange: UndoRedoChange
	{
		ContainerUndoRedoManager manager;
		public string TargetObject;
		public object Diff;
		public ObjectWrapperUndoRedoChange Next;
		
		public ObjectWrapperUndoRedoChange (ContainerUndoRedoManager manager, string targetObject, object diff)
		{
			this.manager = manager;
			this.TargetObject = targetObject;
			this.Diff = diff;
		}
			
		public override UndoRedoChange ApplyChange ()
		{
			return manager.ApplyChange (this);
		}
		
		public override bool CheckValid ()
		{
			return manager.CheckValid ();
		}
	}
	
	class ContainerUndoRedoManager: IDisposable
	{
		UndoQueue queue;
		Wrapper.Container rootObject;
		bool updating;
		UndoManager undoManager = new UndoManager ();
		
		public ContainerUndoRedoManager ()
		{
			undoManager.UndoCheckpoint += OnUndoCheckpoint;
		}
		
		public Wrapper.Container RootObject {
			get { return rootObject; }
			set {
				rootObject = value;
				undoManager.SetRoot (rootObject);
			}
		}
		
		public UndoQueue UndoQueue {
			get { return queue; }
			set { queue = value; }
		}
		
		internal UndoManager UndoManager {
			get { return undoManager; }
		}
		
		void OnUndoCheckpoint (object sender, UndoCheckpointEventArgs args)
		{
			AddChange (args.ModifiedObjects);
		}
		
		void AddChange (ObjectWrapper[] obs)
		{
			if (updating || queue == null)
				return;

			ObjectWrapperUndoRedoChange firstChange = null;
			ObjectWrapperUndoRedoChange lastChange = null;
			
//			Console.WriteLine ("** UNDO CHECKPOINT: {0} objects", obs.Length);
			
			foreach (ObjectWrapper ob in obs) {
				Wrapper.Widget widget = ob as Wrapper.Widget;
				if (widget == null)
					continue;

				// Get the diff for going from the new status to the old status
				object diff = widget.GetUndoDiff ();
				
				if (diff == null)	// No differences
					continue;
				
//				Console.WriteLine ("ADDCHANGE " + widget);
//				Console.WriteLine (diff.ToString ());
				
				ObjectWrapperUndoRedoChange change = new ObjectWrapperUndoRedoChange (this, widget.Wrapped.Name, diff);
				if (lastChange == null)
					lastChange = firstChange = change;
				else {
					lastChange.Next = change;
					lastChange = change;
				}
			}
			if (firstChange != null)
				queue.AddChange (firstChange);
		}
		
		public UndoRedoChange ApplyChange (ObjectWrapperUndoRedoChange first)
		{
			updating = true;
			
			try {
				ObjectWrapperUndoRedoChange change = first;
				ObjectWrapperUndoRedoChange lastRedo = null;
				while (change != null) {
					ObjectWrapperUndoRedoChange redo = ApplyDiff (change.TargetObject, change.Diff);
					if (redo != null) {
						redo.Next = lastRedo;
						lastRedo = redo;
					}
					change = change.Next;
				}
				return lastRedo;
			} catch (Exception ex) {
				Console.WriteLine (ex);
				return null;
			} finally {
				updating = false;
			}
		}
		
		ObjectWrapperUndoRedoChange ApplyDiff (string name, object diff)
		{
//			Console.WriteLine ("** APPLYING DIFF:");
//			Console.WriteLine (diff.ToString ());
			
			Wrapper.Widget ww = rootObject.FindChild (name);
			
			object reverseDiff = ww.ApplyUndoRedoDiff (diff);
		
			if (reverseDiff != null) {
//				Console.WriteLine ("** REVERSE DIFF:");
//				Console.WriteLine (reverseDiff.ToString ());
				
				ObjectWrapperUndoRedoChange change = new ObjectWrapperUndoRedoChange (this, name, reverseDiff);
				return change;
			} else
				return null;
		}
		
		internal bool CheckValid ()
		{
			return rootObject != null;
		}
		
		public void Dispose ()
		{
			rootObject = null;
			if (queue != null)
				queue.Purge ();
		}
	}
}

