using NUnit.Framework;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Kogane.Internal
{
	internal sealed class PrefabOverrideTest
	{
		private sealed class TestResult
		{
			private readonly string m_scenePath;
			private readonly string m_gameObjectHierarchyPath;

			public TestResult( GameObject gameObject )
			{
				m_scenePath               = gameObject.scene.path;
				m_gameObjectHierarchyPath = GetHierarchyPath( gameObject );
			}

			public void WriteTo( StringBuilder builder )
			{
				builder.Append( m_scenePath );
				builder.Append( ',' );
				builder.Append( m_gameObjectHierarchyPath );
				builder.AppendLine();
			}

			private static string GetHierarchyPath( GameObject gameObject )
			{
				var path   = gameObject.name;
				var parent = gameObject.transform.parent;

				while ( parent != null )
				{
					path   = parent.name + "/" + path;
					parent = parent.parent;
				}

				return path;
			}
		}

		[Test]
		public void Test()
		{
			var results = new List<TestResult>();

			GameObjectProcessor.ProcessAllScenes
			(
				scenePathFilter: path => path.StartsWith( "Assets" ),
				onProcess: gameObject =>
				{
					var com = gameObject.GetComponent<PrefabOverrideTestComponent>();

					if ( com == null ) return GameObjectProcessResult.NOT_CHANGE;

					var hasOverrides = PrefabUtility.HasPrefabInstanceAnyOverrides
					(
						instanceRoot: gameObject,
						includeDefaultOverrides: false
					);

					if ( !hasOverrides ) return GameObjectProcessResult.NOT_CHANGE;

					var result = new TestResult( gameObject );

					results.Add( result );

					return GameObjectProcessResult.NOT_CHANGE;
				}
			);

			if ( results.Count <= 0 ) return;

			var builder = new StringBuilder();

			foreach ( var result in results )
			{
				result.WriteTo( builder );
			}

			Assert.Fail( builder.ToString().TrimEnd() );
		}
	}
}