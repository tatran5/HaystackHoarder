using UnityEngine;
using Photon.Pun;

public class PUN2_GasCanSync : MonoBehaviourPun, IPunObservable
{
    // When the object is supposed to "disappear" (gas can is picked up),
    // it is in fact translated in the y direction with the distance set by
    // the variable offsetDisappear
    public bool disappear = false;
    public float offsetDisappear = -5f;  
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {}

    public void callMakeDisappear()
    {
        photonView.RPC("makeDisappear", RpcTarget.AllViaServer);
    }

    public void callMakeAppear(Vector3 position)
    {
        photonView.RPC("makeAppear", RpcTarget.AllViaServer, position);
    }

    [PunRPC]
    public void makeDisappear()
    {
        disappear = true;
        gameObject.transform.position += new Vector3(0, offsetDisappear, 0);
    }

    [PunRPC]
    public void makeAppear(Vector3 position)
    {
        disappear = false;
        gameObject.transform.position = position;
    }        
}