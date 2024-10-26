'use client';

import React, {useState, useEffect} from 'react';
import { Card, Text, Title, Grid, Container, Button } from '@mantine/core';

interface CartItem {
    id: string;
    productId: string;
    productName: string;
    price: number;
    quantity: number;
}

interface Cart {
    items: CartItem[];
    totalPrice: number;
}

export default function() {
    const [cart, setCart] = useState<Cart>({items: [], totalPrice: 0});

    const API_URL = process.env.NEXT_PUBLIC_API_URL;

    useEffect(() => {
        async function fetchCart() {
            let res = await fetch(`${API_URL}/cart`)
            let data = await res.json()
            setCart(data)
        }
        fetchCart()
    }, []);

    const removeFromCart = async (productId: string) => {
        let res = await fetch(`${API_URL}/cart`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({productId})
        })
        let data = await res.json()
        console.log(data)
    }

    return (
        <Container>
          <Title my="xl">Cart</Title>
          <Grid gutter="xl">
            {cart.items.map((item) => (
            <Grid.Col span={6}>
              <Card key={item.id} shadow="xs" padding="xl" radius="lg">
                <Title order={3}>{item.productName}</Title>
                <Text>${item.price} x {item.quantity}</Text>
                <Text>Total: ${item.price * item.quantity}</Text>
                <p/>
                <Button variant='light' size="compact-md" onClick={() => removeFromCart(item.productId)}>Remove</Button>
              </Card>
            </Grid.Col>
            ))}
          </Grid>
          <Title>Total: ${cart.totalPrice}</Title>
        </Container>
      );
}
