import './GlassSuccessModal.css'

function GlassSuccessModal({ open, title, message, details = [], onClose, actionLabel = 'Got it', variant = 'success' }) {
  if (!open) return null;

  const isError = variant === 'error';

  return (
    <div className="glass-modal-overlay" onClick={onClose}>
      <div className={`glass-modal-card ${isError ? 'error' : ''}`} onClick={(e) => e.stopPropagation()}>
        <button className="glass-modal-close" onClick={onClose} aria-label="Close modal">
          ×
        </button>

        <div className="glass-modal-icon-wrap">
          <span className={`glass-modal-icon ${isError ? 'error' : ''}`}>
            {isError ? '✕' : '✓'}
          </span>
        </div>

        <h2 className="glass-modal-title">{title}</h2>
        <p className="glass-modal-message">{message}</p>

        {details.length > 0 && (
          <div className="glass-modal-details">
            {details.map((item, index) => (
              <div key={index} className="glass-modal-detail-item">
                {item}
              </div>
            ))}
          </div>
        )}

        <button className={`glass-modal-action ${isError ? 'error' : ''}`} onClick={onClose}>
          {actionLabel}
        </button>
      </div>
    </div>
  )
}

export default GlassSuccessModal;
