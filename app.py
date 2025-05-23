from flask import Flask, render_template, request, jsonify
import os
from datetime import datetime
import pandas as pd
from evtx import PyEvtxParser
import glob

app = Flask(__name__)

# Log dosyalarının bulunduğu klasör
LOG_DIR = "logs"

def parse_evtx_file(file_path):
    events = []
    parser = PyEvtxParser(file_path)
    for record in parser.records():
        event_data = record.data()
        if 'Event' in event_data:
            event = event_data['Event']
            if 'System' in event and 'EventData' in event:
                system = event['System']
                event_data = event['EventData']
                
                # Event ID'ye göre işlem türünü belirle
                event_id = system.get('EventID', {}).get('#text', '')
                operation_type = "Diğer"
                
                if event_id == '4663':  # Dosya erişimi
                    operation_type = "Erişim"
                elif event_id == '4660':  # Dosya silme
                    operation_type = "Silme"
                elif event_id == '4656':  # Dosya açma
                    operation_type = "Açma"
                
                events.append({
                    'timestamp': system.get('TimeCreated', {}).get('@SystemTime', ''),
                    'user': system.get('Security', {}).get('@UserID', ''),
                    'operation': operation_type,
                    'file_path': event_data.get('ObjectName', ''),
                    'process_name': event_data.get('ProcessName', ''),
                    'computer': system.get('Computer', '')
                })
    return events

@app.route('/')
def index():
    return render_template('index.html')

@app.route('/api/logs', methods=['GET'])
def get_logs():
    log_files = glob.glob(os.path.join(LOG_DIR, "*.evtx"))
    all_events = []
    
    for log_file in log_files:
        events = parse_evtx_file(log_file)
        all_events.extend(events)
    
    # DataFrame oluştur ve sırala
    df = pd.DataFrame(all_events)
    if not df.empty:
        df['timestamp'] = pd.to_datetime(df['timestamp'])
        df = df.sort_values('timestamp', ascending=False)
    
    return jsonify(df.to_dict('records'))

if __name__ == '__main__':
    # Logs klasörünü oluştur
    if not os.path.exists(LOG_DIR):
        os.makedirs(LOG_DIR)
    app.run(debug=True) 