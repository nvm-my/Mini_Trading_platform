import { useState } from 'react';
import {
  View, Text, TextInput, TouchableOpacity, StyleSheet, Alert, Switch, ScrollView,
} from 'react-native';
import { placeOrder } from '../api/orders';

export default function PlaceOrderScreen({ route, navigation }: any) {
  const { instrumentId, symbol } = route.params as { instrumentId: string; symbol: string };
  const [side, setSide] = useState<'BUY' | 'SELL'>('BUY');
  const [isLimit, setIsLimit] = useState(true);
  const [price, setPrice] = useState('');
  const [quantity, setQuantity] = useState('1');

  const handleSubmit = async () => {
    try {
      const order = await placeOrder({
        instrumentId,
        side,
        orderType: isLimit ? 'LIMIT' : 'MARKET',
        price: isLimit ? parseFloat(price) : undefined,
        quantity: parseInt(quantity, 10),
      });
      Alert.alert('Order Placed', `Status: ${order.status}`, [
        { text: 'OK', onPress: () => navigation.goBack() },
      ]);
    } catch {
      Alert.alert('Error', 'Failed to place order. Please try again.');
    }
  };

  return (
    <ScrollView style={styles.container}>
      <Text style={styles.title}>Place Order – {symbol}</Text>
      <View style={styles.toggle}>
        <TouchableOpacity style={[styles.sideBtn, side === 'BUY' && styles.buyActive]} onPress={() => setSide('BUY')}>
          <Text style={[styles.sideBtnText, side === 'BUY' && styles.activeText]}>BUY</Text>
        </TouchableOpacity>
        <TouchableOpacity style={[styles.sideBtn, side === 'SELL' && styles.sellActive]} onPress={() => setSide('SELL')}>
          <Text style={[styles.sideBtnText, side === 'SELL' && styles.activeText]}>SELL</Text>
        </TouchableOpacity>
      </View>
      <View style={styles.row}>
        <Text style={styles.label}>Limit Order</Text>
        <Switch value={isLimit} onValueChange={setIsLimit} />
      </View>
      {isLimit && (
        <TextInput style={styles.input} placeholder="Price" keyboardType="decimal-pad" value={price} onChangeText={setPrice} />
      )}
      <TextInput style={styles.input} placeholder="Quantity" keyboardType="number-pad" value={quantity} onChangeText={setQuantity} />
      <TouchableOpacity style={[styles.button, side === 'SELL' && styles.sellButton]} onPress={handleSubmit}>
        <Text style={styles.buttonText}>Submit {side} Order</Text>
      </TouchableOpacity>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 24, backgroundColor: '#f5f5f5' },
  title: { fontSize: 22, fontWeight: 'bold', marginBottom: 24 },
  toggle: { flexDirection: 'row', marginBottom: 20, gap: 12 },
  sideBtn: { flex: 1, padding: 12, borderRadius: 8, borderWidth: 2, borderColor: '#ccc', alignItems: 'center' },
  buyActive: { borderColor: '#0070f3', backgroundColor: '#e6f0ff' },
  sellActive: { borderColor: '#e00', backgroundColor: '#ffe6e6' },
  sideBtnText: { fontWeight: '600', fontSize: 16, color: '#555' },
  activeText: { color: '#222' },
  row: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 },
  label: { fontSize: 16 },
  input: { backgroundColor: '#fff', borderWidth: 1, borderColor: '#ccc', borderRadius: 8, padding: 12, fontSize: 16, marginBottom: 16 },
  button: { backgroundColor: '#0070f3', borderRadius: 8, padding: 14, alignItems: 'center' },
  sellButton: { backgroundColor: '#e00' },
  buttonText: { color: '#fff', fontSize: 16, fontWeight: '600' },
});
