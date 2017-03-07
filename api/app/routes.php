<?php

/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

$app->get('/authentication', function () use ($app) {
    $app->response->header('Content-Type', 'application/json');

    if (!$app->request->get('username') || !$app->request->get('password') || !$app->request->get('hwid')) {
        return $app->response->write('invalid_usage');
    }

    $username = $app->request->get('username');
    $password = $app->request->get('password');
    $hwid = base64_decode(str_replace(['-', '_', '~'], ['+', '/', '='], $app->request->get('hwid')));

    $user = \MoeApi\Models\User::where('name', $username)->first();
    if (!$user) {
        return $app->response->write('unknown_user');
    }

    if ($password != $user->password) {
        return $app->response->write('invalid_password');
    }

    if ($user->hwid) {
        if ($hwid != $user->hwid) {
            return $app->response->write('invalid_hwid');
        }
        return $app->response->write('success');
    }

    if ($user->status == 'unverified') {
        return $app->response->write('user_unverified');
    }
    if ($user->status == 'banned') {
        return $app->response->write('user_banned');
    }

    $user->update([
          'hwid' => $hwid
    ]);

    $app->response->write('success');
});

$app->get('/rank', function () use ($app) {
    $app->response->header('Content-Type', 'application/json');

    if (!$app->request->get('username')) {
        return $app->response->write('invalid_usage');
    }

    $username = $app->request->get('username');

    $user = \MoeApi\Models\User::where('name', $username)->first();
    if (!$user) {
        return $app->response->write('unknown_user');
    }

    $app->response->write($user->rank);
});

$app->get('/products', function () use ($app) {
    $app->response->header('Content-Type', 'application/json');

    if (!$app->request->get('username')) {
        return $app->response->write('invalid_usage');
    }

    $username = $app->request->get('username');

    $user = \MoeApi\Models\User::where('name', $username)->first();
    if (!$user) {
        return $app->response->write('unknown_user');
    }

    $products = json_decode($user->products, true);

    $final = [];
    foreach ($products as $name => $expiry) {
        $product = \MoeApi\Models\Product::where('name', $name)->where('available', 1)->first();
        if (!$product) {
            continue;
        }

        $final[] = [
          'name' => $product->name,
          'description' => $product->description,
          'expiry' => date('d.m.Y', $expiry),
          'process' => $product->process
        ];
    }

    echo json_encode($final);
});

$app->get('/load', function () use ($app) {
    $app->response->header('Content-Type', 'application/json');

    if (!$app->request->get('username') || !$app->request->get('product')) {
        return $app->response->write('invalid_usage');
    }

    $username = $app->request->get('username');
    $productname = $app->request->get('product');

    $product = \MoeApi\Models\Product::where('name', $productname)->first();
    if (!$product) {
        return $app->response->write('unknown_product');
    }

    $user = \MoeApi\Models\User::where('name', $username)->first();
    if (!$user) {
        return $app->response->write('unknown_user');
    }

    $products = json_decode($user->products, true);
    if (!array_key_exists($productname, $products)) {
        return $app->response->write('no_access');
    }

    if (!file_exists(__DIR__.'/../dlls/'.$product->dllPath)) {
        return $app->response->write('dll_not_present');
    }

    $aes = new \MoeApi\Classes\Aes(null, $product->aesKey, 256);
    $aes->setData(file_get_contents(
        __DIR__.'/../dlls/'.$product->dllPath
    ));

    $app->response->write(base64_encode($aes->decrypt()));
});
