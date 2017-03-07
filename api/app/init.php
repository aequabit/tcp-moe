<?php

/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

$config = (require __DIR__.'/../config.php');

$app = new \Slim\Slim;
$app->config($config['slim']);

$capsule = new Illuminate\Database\Capsule\Manager;
$capsule->addConnection($config['eloquent']);
$capsule->bootEloquent();

require __DIR__.'/routes.php';
