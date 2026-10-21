//using UnityEngine;
//using Wooduduk.Core;

//public class FruitCollector : MonoBehaviour
//{
//    //플레이어 에게 임시로 붙여볼 스크립트, 열매가 잘먹어는지 / 이후 내용은 주용님 가져가 쓰시면 될 것 같습니다. 
//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        IFruit fruit = other.GetComponent<IFruit>();

//        if (fruit == null)
//            return;

//        fruit.OnCollected(gameObject);
//    }
//}
