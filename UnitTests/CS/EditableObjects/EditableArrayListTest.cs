using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

using BLToolkit.TypeBuilder;
using BLToolkit.EditableObjects;
using BLToolkit.Reflection;

using TestObject_1 = TestObject;

namespace EditableObjects
{
	[TestFixture]
	public class EditableArrayListTest
	{
		public abstract class EditableTestObject : EditableObject
		{
			public abstract int    ID      { get; set; }
			public abstract string Name    { get; set; }
			public abstract int    Seconds { get; set; }

			public static EditableTestObject CreateInstance()
			{
				return TypeAccessor.CreateInstance<EditableTestObject>();
			}
			
			public static EditableTestObject CreateInstance(int id, string name, int seconds)
			{
				EditableTestObject instance = TypeAccessor.CreateInstance<EditableTestObject>();

				instance.ID = id;
				instance.Name = name;
				instance.Seconds = seconds;
				
				return instance;
			}

			public override string ToString()
			{
				return string.Format("EditableTestObject - ID:({0}) Name: ({1}) Seconds({2})", ID, Name, Seconds);
			}
		}
		
		private static EditableTestObject[] _testObjects = new EditableTestObject[6]
			{
				EditableTestObject.CreateInstance(0, "Smith",  24),
				EditableTestObject.CreateInstance(1, "John",   22),
				EditableTestObject.CreateInstance(2, "Anna",   48),
				EditableTestObject.CreateInstance(3, "Tim",    56),
				EditableTestObject.CreateInstance(4, "Xiniu",  39),
				EditableTestObject.CreateInstance(5, "Kirill", 30)
			};
		
		public EditableArrayListTest()
		{
			TypeFactory.SaveTypes = true;

			_testList = new EditableArrayList(typeof(EditableTestObject));
			_testList.ListChanged += new ListChangedEventHandler(TestList_ListChanged);
		}

		private EditableArrayList _testList;
		
		private void TestList_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType != ListChangedType.ItemDeleted && e.ListChangedType != ListChangedType.Reset)
				Console.WriteLine("ListChanged (ID:{3}). Type: {0}, OldIndex: {1}, NewIndex: {2}", e.ListChangedType, e.OldIndex, e.NewIndex, (e.NewIndex >= 0 && e.NewIndex < _testList.Count) ? ((EditableTestObject)_testList[e.NewIndex]).ID : -1);
			else
				Console.WriteLine("ListChanged (ID:???). Type: {0}, OldIndex: {1}, NewIndex: {2}", e.ListChangedType, e.OldIndex, e.NewIndex);
		}
		
		[Test]
		public void TestAdd()
		{
			Console.WriteLine("--- TestAdd ---");
			_testList.Clear();
			_testList.RemoveSort();

			for (int i = 0; i < 3; i++)
				_testList.Add(_testObjects[i]);
			
			Assert.AreEqual(_testList.Count, 3);
			Assert.AreEqual(_testList[0], _testObjects[0]);
			Assert.AreEqual(_testList[1], _testObjects[1]);
			Assert.AreEqual(_testList[2], _testObjects[2]);
			
			EditableTestObject[] _subArray = new EditableTestObject[3];

			for (int i = 3; i < _testObjects.Length; i++)
				_subArray[i-3] = _testObjects[i];

			_testList.AddRange(_subArray);
			Assert.AreEqual(_testList.Count, 6);
			
			for (int i = 3; i < _testObjects.Length; i++)
				Assert.AreEqual(_subArray[i - 3], _testObjects[i]);

			PrintList();
			
			_testList.Clear();
		}

		[Test]
		public void TestInsert()
		{
			Console.WriteLine("--- TestInsert ---");
			_testList.Clear();
			_testList.RemoveSort();

			for (int i = 0; i < 3; i++)
				_testList.Insert(0, _testObjects[i]);

			Assert.AreEqual(_testList.Count, 3);
			Assert.AreEqual(_testList[0], _testObjects[2]);
			Assert.AreEqual(_testList[1], _testObjects[1]);
			Assert.AreEqual(_testList[2], _testObjects[0]);

			EditableTestObject[] _subArray = new EditableTestObject[3];

			for (int i = 3; i < _testObjects.Length; i++)
				_subArray[i-3] = _testObjects[i];

			_testList.InsertRange(0, _subArray);
			Assert.AreEqual(_testList.Count, 6);

			Assert.AreEqual(_testList[0], _testObjects[3]);
			Assert.AreEqual(_testList[1], _testObjects[4]);
			Assert.AreEqual(_testList[2], _testObjects[5]);

			PrintList();

			_testList.Clear();
		}
		
		[Test]
		public void TestRemove()
		{
			Console.WriteLine("--- TestRemove ---");

			_testList.Clear();
			_testList.RemoveSort();
			_testList.AddRange(_testObjects);
			
			_testList.RemoveAt(2);
			Assert.AreEqual(_testList.Count, 5);
			_testList.Remove(_testList[0]);
			Assert.AreEqual(_testList.Count, 4);
			_testList.Clear();
			Assert.AreEqual(_testList.Count, 0);
		}

		[Test]
		public void TestSortedAdd()
		{
			Console.WriteLine("--- TestSortedAdd ---");
			_testList.Clear();
			_testList.RemoveSort();
			_testList.ApplySort(_testList.GetItemProperties(null)["Seconds"], ListSortDirection.Descending);

			for (int i = 0; i < 3; i++)
				_testList.Add(_testObjects[i]);

			Assert.AreEqual(_testList.Count, 3);
			Assert.AreEqual(_testList[1], _testObjects[0]);
			Assert.AreEqual(_testList[2], _testObjects[1]);
			Assert.AreEqual(_testList[0], _testObjects[2]);

			EditableTestObject[] _subArray = new EditableTestObject[3];

			for (int i = 3; i < _testObjects.Length; i++)
				_subArray[i-3] = _testObjects[i];

			_testList.AddRange(_subArray);
			Assert.AreEqual(_testList.Count, 6);

			Assert.AreEqual(_testList[0], _testObjects[3]);
			Assert.AreEqual(_testList[2], _testObjects[4]);
			Assert.AreEqual(_testList[3], _testObjects[5]);

			PrintList();

			_testList.Clear();
			Assert.AreEqual(_testList.Count, 0);

			_testList.ApplySort(_testList.GetItemProperties(null)["Seconds"], ListSortDirection.Ascending);

			for (int i = 0; i < 3; i++)
				_testList.Add(_testObjects[i]);

			Assert.AreEqual(_testList.Count, 3);
			Assert.AreEqual(_testList[1], _testObjects[0]);
			Assert.AreEqual(_testList[0], _testObjects[1]);
			Assert.AreEqual(_testList[2], _testObjects[2]);

			_testList.AddRange(_subArray);
			Assert.AreEqual(_testList.Count, 6);

			Assert.AreEqual(_testList[5], _testObjects[3]);
			Assert.AreEqual(_testList[3], _testObjects[4]);
			Assert.AreEqual(_testList[2], _testObjects[5]);

			PrintList();

			_testList.Clear();
		}
		
		[Test]
		public void TestSortedInsert()
		{
			Console.WriteLine("--- TestSortedInsert ---");
			_testList.Clear();
			_testList.RemoveSort();
			_testList.ApplySort(_testList.GetItemProperties(null)["Seconds"], ListSortDirection.Descending);

			for (int i = 0; i < 3; i++)
				_testList.Insert(0, _testObjects[i]);

			Assert.AreEqual(_testList.Count, 3);
			Assert.AreEqual(_testList[0], _testObjects[2]);
			Assert.AreEqual(_testList[1], _testObjects[0]);
			Assert.AreEqual(_testList[2], _testObjects[1]);

			EditableTestObject[] _subArray = new EditableTestObject[3];

			for (int i = 3; i < _testObjects.Length; i++)
				_subArray[i-3] = _testObjects[i];

			_testList.InsertRange(0, _subArray);
			Assert.AreEqual(_testList.Count, 6);

			Assert.AreEqual(_testList[0], _testObjects[3]);
			Assert.AreEqual(_testList[2], _testObjects[4]);
			Assert.AreEqual(_testList[3], _testObjects[5]);

			PrintList();

			_testList.Clear();
			Assert.AreEqual(_testList.Count, 0);

			_testList.ApplySort(_testList.GetItemProperties(null)["Seconds"], ListSortDirection.Ascending);

			for (int i = 0; i < 3; i++)
				_testList.Insert(0, _testObjects[i]);

			Assert.AreEqual(_testList.Count, 3);
			Assert.AreEqual(_testList[0], _testObjects[1]);
			Assert.AreEqual(_testList[1], _testObjects[0]);
			Assert.AreEqual(_testList[2], _testObjects[2]);

			_testList.InsertRange(0, _subArray);
			Assert.AreEqual(_testList.Count, 6);

			Assert.AreEqual(_testList[2], _testObjects[5]);
			Assert.AreEqual(_testList[3], _testObjects[4]);
			Assert.AreEqual(_testList[5], _testObjects[3]);

			PrintList();

			_testList.Clear();
		}
		
		[Test]
		public void TestSortedPropertyChange()
		{
			Console.WriteLine("--- TestSortedPropertyChange ---");
			_testList.Clear();
			_testList.RemoveSort();
			_testList.ApplySort(_testList.GetItemProperties(null)["Seconds"], ListSortDirection.Descending);
			_testList.AddRange(_testObjects);

			EditableTestObject eto = EditableTestObject.CreateInstance(6, "Dummy", 10);

			_testList.Add(eto);
			Assert.AreEqual(_testList.Count, 7);
			Assert.AreEqual(_testList[6], eto);

			eto.Seconds = 20;
			Assert.AreEqual(_testList[6], eto);

			eto.Seconds = 23;
			Assert.AreEqual(_testList[5], eto);

			eto.Seconds = 30;
			Assert.AreEqual(_testList[4], eto);

			eto.Seconds = 40;
			Assert.AreEqual(_testList[2], eto);

			eto.Seconds = 50;
			Assert.AreEqual(_testList[1], eto);

			eto.Seconds = 60;
			Assert.AreEqual(_testList[0], eto);
		}
		
		private void PrintList()
		{
			Console.WriteLine("--- Print List ---");
			foreach (EditableTestObject o in _testList)
				Console.WriteLine(o);
		}

		[Serializable]
		public abstract class SerializableObject : EditableObject
		{
			public abstract int    ID   { get; set; }
			public abstract Guid   UUID { get; set; }
			public abstract string Name { get; set; }
		    
			public abstract EditableList<string> Array { get; set; }
		}

		[Test]
		public void SerializationTest()
		{
			SerializableObject test = TypeAccessor<SerializableObject>.CreateInstance();

			MemoryStream    stream = new MemoryStream();
			BinaryFormatter bf     = new BinaryFormatter();

			bf.Serialize(stream, test);
		}
	}
}
