import { useState, useEffect } from 'react';
import buildingService from '../services/buildingService';
import '../styles/buildings-list.css';

export default function BuildingsList() {
  const [buildings, setBuildings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchBuildings = async () => {
      setLoading(true);
      setError('');
      const result = await buildingService.getAllBuildings();

      if (result.success) {
        setBuildings(result.data || []);
      } else {
        setError(result.error || 'Failed to load buildings');
      }

      setLoading(false);
    };

    fetchBuildings();
  }, []);

  if (loading) return <div className="loading">Loading buildings...</div>;
  if (error) return <div className="error">Error: {error}</div>;

  return (
    <div className="buildings-container">
      <h2>Buildings</h2>

      {buildings.length === 0 ? (
        <p className="no-data">No buildings found</p>
      ) : (
        <div className="buildings-grid">
          {buildings.map((building) => (
            <div key={building.id} className="building-card">
              <h3>{building.name}</h3>
              <p className="address">{building.address}</p>
              <p className="city">{building.city}</p>
              <div className="building-info">
                <span className="units">Units: {building.totalUnits || 0}</span>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
