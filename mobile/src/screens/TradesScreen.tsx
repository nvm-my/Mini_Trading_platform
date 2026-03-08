import { useEffect, useState } from 'react';
import {
  View, Text, FlatList, StyleSheet, ActivityIndicator,
} from 'react-native';
import { getMyTrades, Trade } from '../api/trades';

export default function TradesScreen() {
  const [trades, setTrades] = useState<Trade[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getMyTrades()
      .then(setTrades)
      .finally(() => setLoading(false));
  }, []);

  const renderItem = ({ item }: { item: Trade }) => (
    <View style={styles.card}>
      <Text style={styles.id}>Trade: {item.id.slice(-8)}</Text>
      <Text>Instrument: {item.instrumentId.slice(-8)}</Text>
      <Text>Price: <Text style={styles.price}>{item.price.toFixed(2)}</Text> × {item.quantity}</Text>
      <Text style={styles.date}>{new Date(item.executedAt).toLocaleString()}</Text>
    </View>
  );

  return (
    <View style={styles.container}>
      <Text style={styles.heading}>My Trades</Text>
      {loading ? (
        <ActivityIndicator size="large" color="#0070f3" style={{ marginTop: 40 }} />
      ) : trades.length === 0 ? (
        <Text style={styles.empty}>No trades yet.</Text>
      ) : (
        <FlatList data={trades} keyExtractor={(t) => t.id} renderItem={renderItem} />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f5f5' },
  heading: { fontSize: 22, fontWeight: 'bold', padding: 16, backgroundColor: '#fff', borderBottomWidth: 1, borderColor: '#eee' },
  card: { backgroundColor: '#fff', margin: 8, marginHorizontal: 16, borderRadius: 8, padding: 16, gap: 4 },
  id: { fontWeight: 'bold', fontSize: 14, color: '#555' },
  price: { fontWeight: 'bold', color: '#0070f3' },
  date: { fontSize: 12, color: '#999', marginTop: 4 },
  empty: { textAlign: 'center', marginTop: 40, color: '#888', fontSize: 16 },
});
