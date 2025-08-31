using System.Collections;
using NgoUyenNguyen.GridSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RuntimeTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void RuntimeTestSimplePasses()
    {
        // Use the Assert class to test conditions
        GameObject go = new GameObject();
        BaseGrid grid = go.AddComponent<BaseGrid>();
        grid.transform.position = Vector3.zero;
        grid.cellSize = 1;

        TestHorizontalSpace(grid);
        TestVerticalSpace(grid);
    }




    private static void TestHorizontalSpace(BaseGrid grid)
    {
        grid.space = BaseGrid.Space.Horizontal;
        grid.alignment = BaseGrid.Alignment.BottomLeft;
        Assert.IsTrue(grid.IndexToLocal(new Vector2Int(1,1)) == new Vector3(1.5f, 0, 1.5f));
    }
    private static void TestVerticalSpace(BaseGrid grid)
    {
        grid.space = BaseGrid.Space.Horizontal;
        grid.alignment = BaseGrid.Alignment.BottomLeft;
        Assert.IsTrue(grid.IndexToLocal(new Vector2Int(1,1)) == new Vector3(1.5f, 1.5f, 0));
    }






    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator RuntimeTestWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
