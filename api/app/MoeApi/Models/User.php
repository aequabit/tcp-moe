<?php

/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

namespace MoeApi\Models;

use Illuminate\Database\Eloquent\Model as Eloquent;

class User extends Eloquent {
  public $fillable = [ 'hwid' ];
}
