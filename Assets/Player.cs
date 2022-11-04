using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private Ball _prefabBall;
    [SerializeField] private PhysxBall _prefabPhysxBall;

    private Vector3 _forward;
    [Networked] TickTimer delay { get;set; }


    private NetworkCharacterControllerPrototype _cc;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterControllerPrototype>();
    }

    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            _cc.Move(5 * data.direction * Runner.DeltaTime);

            if(data.direction.sqrMagnitude>0)
            {
                _forward = data.direction;
            }

            if(delay.ExpiredOrNotRunning(Runner))
            {
                if((data.buttons & NetworkInputData.MOUSEBUTTON1) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabBall,
                        transform.position+_forward, Quaternion.LookRotation(_forward),
                        Object.InputAuthority, (runner, o) =>{
                            o.GetComponent<Ball>().Init();
                        });
                }
                else if((data.buttons & NetworkInputData.MOUSEBUTTON2) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabPhysxBall,
                        transform.position+_forward, Quaternion.LookRotation(_forward),
                        Object.InputAuthority, (runner, o) =>{
                            o.GetComponent<PhysxBall>().Init(_forward * 10);
                        });
                }
            }
        }
    }
}
