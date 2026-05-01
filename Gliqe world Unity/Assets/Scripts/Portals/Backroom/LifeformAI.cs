using UnityEngine;
using UnityEngine.AI;

namespace GliqeWorld.Portals.Backroom
{
    public enum LifeformState { Patrol, Alert, Chase, Stunned }

    /// <summary>
    /// NavMesh-based pursuit FSM for the Backroom monster.
    /// Transitions: Patrol → Alert (sound/sight) → Chase (direct) → Stunned (scissors hit).
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class LifeformAI : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [Header("Movement")]
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float chaseSpeed = 6f;
        [SerializeField] private float alertAcceleration = 4f;

        [Header("Detection")]
        [SerializeField] private float hearingRange = 12f;
        [SerializeField] private float visionRange = 8f;
        [SerializeField] private float visionAngle = 60f;
        [SerializeField] private float losePlayerTimeout = 5f;

        [Header("Waypoints")]
        [SerializeField] private Transform[] patrolWaypoints;

        [Header("References")]
        [SerializeField] private AudioSource footstepAudio;

        // ── State ────────────────────────────────────────────────────────────────

        public LifeformState CurrentState { get; private set; } = LifeformState.Patrol;

        private NavMeshAgent _agent;
        private Transform _player;
        private int _waypointIndex;
        private float _loseTimer;

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _player = playerObj.transform;
        }

        private void Update()
        {
            switch (CurrentState)
            {
                case LifeformState.Patrol:  UpdatePatrol();  break;
                case LifeformState.Alert:   UpdateAlert();   break;
                case LifeformState.Chase:   UpdateChase();   break;
                case LifeformState.Stunned: break;
            }
        }

        // ── States ───────────────────────────────────────────────────────────────

        private void UpdatePatrol()
        {
            _agent.speed = patrolSpeed;

            if (patrolWaypoints != null && patrolWaypoints.Length > 0)
            {
                _agent.SetDestination(patrolWaypoints[_waypointIndex].position);

                if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
                    _waypointIndex = (_waypointIndex + 1) % patrolWaypoints.Length;
            }

            if (CanHearPlayer() || CanSeePlayer())
                SetState(LifeformState.Alert);
        }

        private void UpdateAlert()
        {
            _agent.speed = Mathf.MoveTowards(_agent.speed, chaseSpeed, alertAcceleration * Time.deltaTime);

            if (_player != null)
                _agent.SetDestination(_player.position);

            if (CanSeePlayer())
                SetState(LifeformState.Chase);
        }

        private void UpdateChase()
        {
            _agent.speed = chaseSpeed;

            if (_player != null)
                _agent.SetDestination(_player.position);

            bool playerInSight = CanSeePlayer();
            if (!playerInSight)
            {
                _loseTimer += Time.deltaTime;
                if (_loseTimer >= losePlayerTimeout)
                    SetState(LifeformState.Patrol);
            }
            else
            {
                _loseTimer = 0f;
            }
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Called by the Scissors item on use within 2m.
        /// Triggers the stunned/death state and despawns the Lifeform.
        /// </summary>
        public void OnStunnedByScissors()
        {
            SetState(LifeformState.Stunned);
            _agent.isStopped = true;
            // Play death anim then despawn — animator trigger via Animator component
            Animator anim = GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("Death");
            Invoke(nameof(Despawn), 3f);
        }

        // ── Detection ────────────────────────────────────────────────────────────

        private bool CanHearPlayer()
        {
            if (_player == null || footstepAudio == null) return false;
            return footstepAudio.isPlaying
                && Vector3.Distance(transform.position, _player.position) <= hearingRange;
        }

        private bool CanSeePlayer()
        {
            if (_player == null) return false;

            Vector3 toPlayer = _player.position - transform.position;
            float dist = toPlayer.magnitude;

            if (dist > visionRange) return false;
            if (Vector3.Angle(transform.forward, toPlayer) > visionAngle * 0.5f) return false;

            return !Physics.Raycast(transform.position + Vector3.up, toPlayer.normalized, dist);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void SetState(LifeformState state)
        {
            CurrentState = state;
            _loseTimer = 0f;
        }

        private void Despawn() => gameObject.SetActive(false);
    }
}
