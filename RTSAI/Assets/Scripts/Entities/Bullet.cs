using UnityEngine;

namespace RTS
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float _lifeTime = 0.5f;
        [SerializeField] private float _moveForce = 2000f;

        private float _shootDate;
        private Unit _unitOwner;
        private BaseEntity _target;
        private int _damages;

        public void ShootToward(BaseEntity target, Unit owner, int damages)
        {
            _shootDate = Time.time;
            Vector3 direction = target.transform.position - transform.position;
            GetComponent<Rigidbody>().AddForce(direction.normalized * _moveForce);
            _target = target;
            _unitOwner = owner;
            _damages = damages;
        }

        #region MonoBehaviour methods
        void Update()
        {
            if ((Time.time - _shootDate) > _lifeTime)
            {
                DestroyBullet();
            }
        }
        void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.GetComponent<Unit>()?.GetTeam() == _unitOwner.GetTeam())
                return;

            DestroyBullet();
        }

        void DestroyBullet()
        {
            _target.AddDamage(_damages);
            Destroy(gameObject);
        }
        #endregion
    }
}