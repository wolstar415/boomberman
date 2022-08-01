using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoomController : MonoBehaviour
{
    

    public void Start()
    {
        SocketManager.inst.socket.OnUnityThread("Bomb", data =>
        {
            GameObject bomb = ObjectPooler.SpawnFromPool(BoomberManager.inst.bombPrefab, new Vector3(data.GetValue(3).GetSingle(),data.GetValue(4).GetSingle()), quaternion.identity);
            if (bomb.TryGetComponent(out Boom component))
            {
                component.BoomFunc(data.GetValue(0).GetSingle(),data.GetValue(1).GetInt32(),data.GetValue(2).GetInt32());
            }
        });
    }

    private void OnBomb(InputValue value)
    //스페이스를 누르면 발동
    {
        if (BoomberManager.inst.IsStart == false||BoomberManager.inst.IsDead)
            //시작을 안했거나 죽으면 발동 X
        {
            return;
        }
        
        if (!BoomberManager.inst.playChat.isFocused&&BoomberManager.inst.bombsRemaining >0)
            //채팅중에는 폭탄이 안나오고 설치할 수 있는 폭탄 개수 체크
        {
            Vector2 position = BoomberManager.inst.player.transform.position;
            //현재 위치를 저장합니다.
            position.x = Mathf.Round(position.x);
            position.y = Mathf.Round(position.y);
            //타일안에 정확하게 들어가야 하기 때문에 반올림합니다.

            if (Physics2D.OverlapCircle(position, 0.2f, BoomberManager.inst.explodeMask))
                //그 타일에 폭탄이 있다면 설치 X
            {
                return;
            }
       
            
            BoomberManager.inst.bombsRemaining--;
            GameObject bomb = ObjectPooler.SpawnFromPool(BoomberManager.inst.bombPrefab, position, Quaternion.identity);
            SocketManager.inst.socket.Emit("Bomb",GameManager.inst.room,BoomberManager.inst.bombFuseTime,BoomberManager.inst.Power,BoomberManager.inst.playerIdx,position.x,position.y);
            if (bomb.TryGetComponent(out Boom component))
            {
                component.BoomFunc(BoomberManager.inst.bombFuseTime,BoomberManager.inst.Power,BoomberManager.inst.playerIdx);
            }
            
        }
    }
    
}
