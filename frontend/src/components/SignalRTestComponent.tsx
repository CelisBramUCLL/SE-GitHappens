import React, { useEffect, useState } from 'react';
import { signalRService } from '../services/signalRService';

export const SignalRTestComponent: React.FC = () => {
  const [status, setStatus] = useState('Initializing...');
  const [events, setEvents] = useState<string[]>([]);
  const [isConnected, setIsConnected] = useState(false);

  const addEvent = (msg: string) => {
    const timestamp = new Date().toLocaleTimeString();
    setEvents(prev => [...prev, `[${timestamp}] ${msg}`]);
  };

  useEffect(() => {
    let mounted = true;

    const testConnection = async () => {
      try {
        addEvent('🔄 Starting SignalR connection test...');
        setStatus('Connecting...');
        
        await signalRService.connect();
        
        if (mounted) {
          setStatus('Connected');
          setIsConnected(true);
          addEvent('✅ SignalR connected successfully!');

          // Set up event listeners
          signalRService.onUserJoinedParty((userId, partyId) => {
            addEvent(`🟢 UserJoinedParty: User ${userId} joined Party ${partyId}`);
          });

          signalRService.onUserLeftParty((userId, partyId) => {
            addEvent(`🔴 UserLeftParty: User ${userId} left Party ${partyId}`);
          });

          // Test joining a party
          setTimeout(async () => {
            try {
              addEvent('📞 Testing JoinParty(1)...');
              await signalRService.joinParty(1);
              addEvent('✅ JoinParty(1) successful');
            } catch (error) {
              addEvent(`❌ JoinParty(1) failed: ${error}`);
            }
          }, 1000);
        }
      } catch (error) {
        if (mounted) {
          setStatus('Connection Failed');
          addEvent(`❌ Connection failed: ${error}`);
          console.error('SignalR connection error:', error);
        }
      }
    };

    testConnection();

    return () => {
      mounted = false;
      signalRService.disconnect();
    };
  }, []);

  return (
    <div className="p-4 border border-gray-300 rounded-lg bg-gray-50 mb-4">
      <h3 className="font-bold text-lg mb-2">SignalR Connection Test</h3>
      
      <div className="mb-2">
        <span className="font-semibold">Status: </span>
        <span className={isConnected ? 'text-green-600' : 'text-red-600'}>
          {status}
        </span>
      </div>

      <div className="bg-white p-2 rounded border max-h-40 overflow-y-auto">
        {events.length === 0 ? (
          <div className="text-gray-500 text-sm">No events yet...</div>
        ) : (
          events.map((event, idx) => (
            <div key={idx} className="text-sm font-mono">{event}</div>
          ))
        )}
      </div>

      <div className="mt-2">
        <button 
          onClick={async () => {
            try {
              addEvent('📞 Manual JoinParty(1) test...');
              await signalRService.joinParty(1);
              addEvent('✅ Manual JoinParty(1) successful');
            } catch (error) {
              addEvent(`❌ Manual JoinParty(1) failed: ${error}`);
            }
          }}
          disabled={!isConnected}
          className="bg-blue-500 text-white px-3 py-1 rounded text-sm disabled:bg-gray-300"
        >
          Test Join Party
        </button>
      </div>
    </div>
  );
};