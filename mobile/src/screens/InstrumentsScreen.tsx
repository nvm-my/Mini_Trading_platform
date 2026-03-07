import { useEffect, useState } from 'react';
import {
  View, Text, FlatList, TouchableOpacity, StyleSheet, ActivityIndicator,
} from 'react-native';
import { getInstruments, Instrument } from '../api/instruments';
import { useAuth } from '../context/AuthContext';

export default function InstrumentsScreen({ navigation }: any) {
  const { signOut } = useAuth();
  const [instruments, setInstruments] = useState<Instrument[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getInstruments()
      .then(setInstruments)
      .finally(() => setLoading(false));
  }, []);

  const renderItem = ({ item }: { item: Instrument }) => (
    <TouchableOpacity
      style={styles.row}
      onPress={() => navigation.navigate('PlaceOrder', { instrumentId: item.id, symbol: item.symbol })}
    >
      <View>
        <Text style={styles.symbol}>{item.symbol}</Text>
        <Text style={styles.company}>{item.companyName}</Text>
      </View>
      <Text style={styles.price}>{item.currentPrice.toFixed(2)}</Text>
    </TouchableOpacity>
  );

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.heading}>Instruments</Text>
        <View style={styles.headerActions}>
          <TouchableOpacity onPress={() => navigation.navigate('Trades')}>
            <Text style={styles.navLink}>Trades</Text>
          </TouchableOpacity>
          <TouchableOpacity onPress={signOut}>
            <Text style={[styles.navLink, { color: '#e00' }]}>Sign Out</Text>
          </TouchableOpacity>
        </View>
      </View>
      {loading ? (
        <ActivityIndicator size="large" color="#0070f3" style={{ marginTop: 40 }} />
      ) : (
        <FlatList data={instruments} keyExtractor={(i) => i.id} renderItem={renderItem} />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f5f5' },
  header: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', padding: 16, backgroundColor: '#fff', borderBottomWidth: 1, borderColor: '#eee' },
  heading: { fontSize: 22, fontWeight: 'bold' },
  headerActions: { flexDirection: 'row', gap: 16 },
  navLink: { color: '#0070f3', fontSize: 16 },
  row: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', backgroundColor: '#fff', margin: 8, marginHorizontal: 16, borderRadius: 8, padding: 16 },
  symbol: { fontSize: 18, fontWeight: 'bold' },
  company: { fontSize: 14, color: '#555', marginTop: 2 },
  price: { fontSize: 20, fontWeight: '600', color: '#0070f3' },
});
